using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class AutomationBase(ILogger logger, SwitchEntity? masterSwitch = null)
    : IAutomation,
        IDisposable
{
    protected SwitchEntity? MasterSwitch { get; } = masterSwitch;
    protected ILogger Logger { get; } = logger;
    protected abstract IEnumerable<IDisposable> GetToggleableAutomations();
    protected abstract IEnumerable<IDisposable> GetPersistentAutomations();

    protected virtual void RunInitialActions() { }

    private CompositeDisposable? _toggleableAutomations;
    private CompositeDisposable? _persistentAutomations;

    public virtual void StartAutomation()
    {
        try
        {
            var persistentAutomations = GetPersistentAutomations();
            var persistentList = new List<IDisposable>(persistentAutomations);
            Logger.LogDebug(
                "Starting {AutomationType} with {PersistentCount} persistent automations",
                GetType().Name,
                persistentList.Count
            );

            _persistentAutomations = [.. persistentList];

            if (MasterSwitch is not null)
            {
                Logger.LogDebug(
                    "Configuring master switch monitoring for {EntityId}",
                    MasterSwitch.EntityId
                );
                _persistentAutomations.Add(
                    MasterSwitch
                        .StateChangesWithCurrent()
                        .SubscribeSafe(
                            ToggleAutomation,
                            onError: ex =>
                                Logger.LogError(
                                    ex,
                                    "Error in master switch subscription for {EntityId}",
                                    MasterSwitch.EntityId
                                )
                        )
                );
            }

            Logger.LogInformation("{AutomationType} started successfully", GetType().Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start automation {AutomationType}", GetType().Name);
            throw;
        }
    }

    private void EnableAutomations()
    {
        if (_toggleableAutomations != null)
        {
            Logger.LogDebug(
                "Toggleable automations already enabled for {AutomationType}",
                GetType().Name
            );
            return;
        }

        var toggleableAutomations = GetToggleableAutomations();
        var toggleableList = new List<IDisposable>(toggleableAutomations);
        Logger.LogDebug(
            "Enabling {Count} toggleable automations for {AutomationType}",
            toggleableList.Count,
            GetType().Name
        );
        _toggleableAutomations = [.. toggleableList];
        RunInitialActions();
    }

    private void DisableAutomations()
    {
        var count = _toggleableAutomations?.Count ?? 0;
        if (count > 0)
        {
            Logger.LogDebug(
                "Disabling {Count} toggleable automations for {AutomationType}",
                count,
                GetType().Name
            );
        }
        _toggleableAutomations?.Dispose();
        _toggleableAutomations = null;
    }

    private void ToggleAutomation(StateChange e)
    {
        Logger.LogDebug(
            "Master switch state change: {OldState} → {NewState} for {EntityId} by {UserId}",
            e.Old?.State,
            e.New?.State,
            MasterSwitch?.EntityId,
            e.Username() ?? "unknown"
        );

        if (MasterSwitch?.State != HaEntityStates.ON)
        {
            Logger.LogDebug(
                "Master switch OFF - disabling automations for {AutomationType}",
                GetType().Name
            );
            DisableAutomations();
            return;
        }

        Logger.LogDebug(
            "Master switch ON - enabling automations for {AutomationType}",
            GetType().Name
        );
        EnableAutomations();
    }

    public virtual void Dispose()
    {
        Logger.LogDebug("Disposing {AutomationType} automation", GetType().Name);
        _persistentAutomations?.Dispose();
        _persistentAutomations = null;
        DisableAutomations();
        GC.SuppressFinalize(this);
    }
}
