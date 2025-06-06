using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen;

[NetDaemonApp]
public class Kitchen : MotionAutomationBase
{
    private readonly BinarySensorEntity _motionSensor;
    private readonly BinarySensorEntity _powerPlug;
    private readonly LightEntity _light;
    private readonly NumberEntity _sensorDelay;
    private readonly SwitchEntity _enableMotionSensor;

    public Kitchen(Entities entities, IHaContext ha)
        : base(entities.Switch.KitchenMotionSensor)
    {
        _motionSensor = entities.BinarySensor.KitchenMotionSensors;
        _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
        _light = entities.Light.RgbLightStrip;
        _sensorDelay = entities.Number.Ld2410Esp325StillTargetDelay;
        _enableMotionSensor = entities.Switch.KitchenMotionSensor;

        SetupMotionSensorReactivation();
        UpdateAutomationsBasedOnSwitch();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        const int SensorWaitTime = 30;
        const int SensorDelayValueActive = 15;
        const int SensorDelayValueInactive = 1;

        // Lighting automation
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, 5).Subscribe(_ => _light.TurnOn());
        yield return _motionSensor.StateChanges().IsOff().Subscribe(_ => _light.TurnOff());

        // Sensor delay automation
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueInactive));
        yield return _powerPlug.StateChanges().IsOn().Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueActive));
    }

    private void SetupMotionSensorReactivation()
    {
        _motionSensor.StateChanges().WhenStateIsForHours(HaEntityStates.OFF, 1).Subscribe(_ => _enableMotionSensor.TurnOn());
    }
}
