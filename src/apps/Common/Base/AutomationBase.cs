using System.Reactive.Disposables;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common.Base;

public abstract class AutomationBase(ILogger logger, SwitchEntity? masterSwitch = null) : IAutomation, IDisposable
{
    protected SwitchEntity? MasterSwitch { get; } = masterSwitch;
    protected ILogger Logger { get; } = logger;
    protected abstract IEnumerable<IDisposable> GetToggleableAutomations();
    protected abstract IEnumerable<IDisposable> GetPersistentAutomations();
    private CompositeDisposable? _toggleableAutomations;
    private CompositeDisposable? _persistentAutomations;

    public virtual void StartAutomation()
    {
        _persistentAutomations = [.. GetPersistentAutomations()];

        if (MasterSwitch is not null)
        {
            _persistentAutomations.Add(MasterSwitch.StateAllChangesWithCurrent().Subscribe(ToggleAutomation));
        }
    }

    private void EnableAutomations()
    {
        if (_toggleableAutomations != null)
        {
            return;
        }
        _toggleableAutomations = [.. GetToggleableAutomations()];
    }

    private void DisableAutomations()
    {
        _toggleableAutomations?.Dispose();
        _toggleableAutomations = null;
    }

    private void ToggleAutomation(StateChange e)
    {
        if (MasterSwitch?.State != HaEntityStates.ON)
        {
            DisableAutomations();
            return;
        }
        EnableAutomations();
    }

    public virtual void Dispose()
    {
        _persistentAutomations?.Dispose();
        _persistentAutomations = null;
        DisableAutomations();
        GC.SuppressFinalize(this);
    }
}
