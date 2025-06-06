using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Kitchen;

[NetDaemonApp]
public class Kitchen : IDisposable
{
    private readonly BinarySensorEntity _motionSensor;
    private readonly BinarySensorEntity _powerPlug;
    private readonly LightEntity _light;
    private readonly NumberEntity _sensorDelay;
    private readonly SwitchEntity _enableMotionSensor;
    private CompositeDisposable? _automations;
    private IDisposable? _switchSubscription;

    public Kitchen(Entities entities, IHaContext ha)
    {
        _motionSensor = entities.BinarySensor.KitchenMotionSensors;
        _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
        _light = entities.Light.RgbLightStrip;
        _sensorDelay = entities.Number.Ld2410Esp325StillTargetDelay;
        _enableMotionSensor = entities.Switch.KitchenMotionSensor;

        SubscribeToEnableSwitch();
        ApplyAutomationState();
        SetupMotionSensorReactivation();
    }
    private void ApplyAutomationState()
    {
        if (IsSwitchOn())
        {
            EnableAutomations();
        }
        else
        {
            DisableAutomations();
        }
    }

    public void EnableAutomations()
    {
        if (_automations != null)
        {
            return;
        }

        _automations = [.. SetupMotionTriggeredLighting()
            .Concat(SetupSensorDelayAdjustment())];
    }

    public void DisableAutomations()
    {
        _automations?.Dispose();
        _automations = null;
    }

    public void Dispose()
    {
        DisableAutomations();
        _switchSubscription?.Dispose();
    }

    private void SubscribeToEnableSwitch()
    {
        _switchSubscription = _enableMotionSensor
            .StateChanges()
            .Subscribe(e => ApplyAutomationState());
    }

    private bool IsSwitchOn() => _enableMotionSensor.State == HaEntityStates.ON;

    private IEnumerable<IDisposable> SetupMotionTriggeredLighting()
    {
        yield return _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, 5)
            .Subscribe(_ => _light.TurnOn());

        yield return _motionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(_ => _light.TurnOff());
    }

    private IEnumerable<IDisposable> SetupSensorDelayAdjustment()
    {
        const int MotionSustainedDuration = 30;
        const int DelayWhenActive = 15;
        const int DelayWhenInactive = 1;

        yield return _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, MotionSustainedDuration)
            .Subscribe(_ => _sensorDelay.SetNumericValue(DelayWhenActive));

        yield return _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, MotionSustainedDuration)
            .Subscribe(_ => _sensorDelay.SetNumericValue(DelayWhenInactive));

        yield return _powerPlug
            .StateChanges()
            .IsOn()
            .Subscribe(_ => _sensorDelay.SetNumericValue(DelayWhenActive));
    }

    private void SetupMotionSensorReactivation()
    {
        _motionSensor
            .StateChanges()
            .WhenStateIsForHours(HaEntityStates.OFF, 1)
            .Subscribe(_ => _enableMotionSensor.TurnOn());
    }
}
