using System.Collections.Generic;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common;

public abstract class MotionAutomationBase : IDisposable
{
    private readonly SwitchEntity _enableSwitch;
    private IDisposable? _switchSubscription;
    private CompositeDisposable? _automations;

    protected MotionAutomationBase(SwitchEntity enableSwitch)
    {
        _enableSwitch = enableSwitch;
        _switchSubscription = _enableSwitch.StateChanges().Subscribe(_ => ApplyAutomationState());
        ApplyAutomationState();
    }

    private void ApplyAutomationState()
    {
        if (_enableSwitch.State == HaEntityStates.ON)
        {
            EnableAutomations();
        }
        else
        {
            DisableAutomations();
        }
    }

    private void EnableAutomations()
    {
        if (_automations != null)
        {
            return;
        }
        _automations = [.. GetAutomations()];
    }

    private void DisableAutomations()
    {
        _automations?.Dispose();
        _automations = null;
    }

    protected abstract IEnumerable<IDisposable> GetAutomations();

    public virtual void Dispose()
    {
        DisableAutomations();
        _switchSubscription?.Dispose();
    }
}