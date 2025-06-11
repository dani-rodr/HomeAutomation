using System.Collections.Generic;
using System.Reactive.Disposables;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common;

public abstract class AutomationBase(ILogger logger, SwitchEntity? masterSwitch = null) : IAutomation, IDisposable
{
    protected SwitchEntity? MasterSwitch { get; } = masterSwitch;
    protected ILogger Logger { get; } = logger;
    protected abstract IEnumerable<IDisposable> GetSwitchableAutomations();
    protected abstract IEnumerable<IDisposable> GetStartupAutomations();
    private CompositeDisposable? _toggleableAutomations;
    private CompositeDisposable? _permanentAutomations;

    public virtual void StartAutomation()
    {
        _permanentAutomations = [.. GetStartupAutomations()];

        if (MasterSwitch is not null)
        {
            _permanentAutomations.Add(MasterSwitch.StateAllChangesWithCurrent().Subscribe(ToggleAutomation));
        }
    }

    private void EnableAutomations()
    {
        if (_toggleableAutomations != null)
        {
            return;
        }
        _toggleableAutomations = [.. GetSwitchableAutomations()];
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
        _permanentAutomations?.Dispose();
        _permanentAutomations = null;
        DisableAutomations();
        GC.SuppressFinalize(this);
    }
}
