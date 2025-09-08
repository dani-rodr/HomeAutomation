using System.Linq;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class ToggleableAutomation(SwitchEntity masterSwitch, ILogger logger)
    : IAutomation,
        IDisposable
{
    protected readonly SwitchEntity MasterSwitch = masterSwitch;
    protected readonly ILogger Logger = logger;
    protected abstract IEnumerable<IDisposable> GetToggleableAutomations();
    protected abstract IEnumerable<IDisposable> GetPersistentAutomations();

    protected virtual void RunInitialActions() { }

    private CompositeDisposable? _toggleableAutomations;
    private CompositeDisposable? _persistentAutomations;

    public virtual void StartAutomation()
    {
        if (_persistentAutomations is not null)
        {
            _persistentAutomations.Dispose();
            _persistentAutomations = null;
            Logger.LogDebug(
                "Restarting persistent automations for {AutomationType}",
                GetType().Name
            );
        }
        try
        {
            _persistentAutomations = [.. GetPersistentAutomations()];
            Logger.LogDebug(
                "Starting {AutomationType} with {PersistentCount} persistent automations",
                GetType().Name,
                _persistentAutomations.Count()
            );

            if (MasterSwitch is null)
            {
                Logger.LogDebug("MasterSwitch for {AutomationType} is null", GetType().Name);
                return;
            }
            Logger.LogDebug(
                "Configuring master switch monitoring for {EntityId}",
                MasterSwitch.EntityId
            );
            _persistentAutomations.Add(
                MasterSwitch
                    .OnChanges(new(AllowFromUnavailable: false, StartImmediately: true))
                    .Subscribe(ToggleAutomation)
            );
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

        _toggleableAutomations = [.. GetToggleableAutomations()];

        Logger.LogDebug(
            "Enabling {Count} toggleable automations for {AutomationType}",
            _toggleableAutomations.Count,
            GetType().Name
        );
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
            MasterSwitch.EntityId,
            e.Username() ?? "unknown"
        );

        if (MasterSwitch.IsOff())
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
