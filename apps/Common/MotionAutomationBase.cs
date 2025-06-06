using System.Collections.Generic;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common;

public abstract class MotionAutomationBase : IDisposable
{
    protected readonly BinarySensorEntity _motionSensor;
    protected readonly LightEntity _light;
    protected readonly SwitchEntity _enableSwitch;
    protected readonly NumberEntity _sensorDelay;
    protected abstract IEnumerable<IDisposable> GetAutomations();

    private IDisposable? _switchSubscription;
    private CompositeDisposable? _automations;


    // Update constructor to accept BinarySensorEntity
    protected MotionAutomationBase(SwitchEntity enableSwitch, BinarySensorEntity motionSensor, LightEntity light, NumberEntity sensorDelay)
    {
        _enableSwitch = enableSwitch;
        _motionSensor = motionSensor;
        _light = light;
        _sensorDelay = sensorDelay;

        _switchSubscription = _enableSwitch.StateChanges().Subscribe(_ => InitializeAutomations());
        // Do not call UpdateAutomationsBasedOnSwitch() here!
    }

    /// <summary>
    /// Must be called at the end of the derived class constructor after all fields are initialized.
    /// </summary>
    protected void InitializeAutomations()
    {
        if (_enableSwitch.State != HaEntityStates.ON)
        {
            DisableAutomations();
            return;
        }

        EnableAutomations();
        if (_motionSensor.State == HaEntityStates.ON)
        {
            _light.TurnOn();
        }
        else
        {
            _light.TurnOff();
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

    public virtual void Dispose()
    {
        DisableAutomations();
        _switchSubscription?.Dispose();
    }
}