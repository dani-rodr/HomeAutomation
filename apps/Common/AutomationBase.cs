using System.Collections.Generic;
using System.Reactive.Disposables;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common;

public abstract class AutomationBase(ILogger logger, SwitchEntity? masterSwitch = null) : IAutomation, IDisposable
{
    protected SwitchEntity? MasterSwitch { get; } = masterSwitch;
    protected ILogger Logger { get; } = logger;
    protected abstract IEnumerable<IDisposable> GetSwitchableAutomations();
    private CompositeDisposable? _automations;

    public virtual void StartAutomation()
    {
        MasterSwitch?.StateAllChangesWithCurrent().Subscribe(ToggleAutomation);
    }

    private void EnableAutomations()
    {
        if (_automations != null)
        {
            return;
        }
        _automations = [.. GetSwitchableAutomations()];
    }

    private void DisableAutomations()
    {
        _automations?.Dispose();
        _automations = null;
    }
    protected void RestartAutomations()
    {
        DisableAutomations();
        EnableAutomations();
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
        DisableAutomations();
        GC.SuppressFinalize(this);
    }
}
