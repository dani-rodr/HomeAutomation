using System.Collections.Generic;
using System.Reactive.Disposables;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common;

public abstract class AutomationBase(ILogger logger, SwitchEntity? masterSwitch = null) : IAutomation, IDisposable
{
    protected SwitchEntity? MasterSwitch { get; } = masterSwitch;
    protected ILogger Logger { get; } = logger;
    protected abstract IEnumerable<IDisposable> SwitchableAutomations();
    private CompositeDisposable? _automations;

    public virtual void StartAutomation()
    {
        ToggleAutomation();
        MasterSwitch?.StateChanges().Subscribe(_ => ToggleAutomation());
    }

    private void EnableAutomations()
    {
        if (_automations != null)
        {
            return;
        }
        _automations = [.. SwitchableAutomations()];
    }

    private void DisableAutomations()
    {
        _automations?.Dispose();
        _automations = null;
    }

    protected virtual void ToggleAutomation()
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
