using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen;

[NetDaemonApp]
public class Pantry : MotionAutomationBase
{
    protected override int SensorWaitTime => 10;
    private readonly BinarySensorEntity _miScaleStepSensor;
    private readonly LightEntity _mirrorLight;
    public Pantry(Entities entities)
        : base(entities.Switch.PantryMotionSensor,
               entities.BinarySensor.PantryMotionSensors,
               entities.Light.PantryLights,
               entities.Number.ZEsp32C63StillTargetDelay)
    {
        _miScaleStepSensor = entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
        _mirrorLight = entities.Light.ControllerRgbDf1c0d;

        InitializeMotionAutomation();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        // Lighting automation
        yield return _motionSensor.StateChanges().IsOn().Subscribe(_ => _light.TurnOn());
        yield return _motionSensor.StateChanges().IsOff().Subscribe(_ =>
        {
            _light.TurnOff();
            _mirrorLight.TurnOff();
        });
        yield return _miScaleStepSensor.StateChanges().IsOn().Subscribe(_ => _mirrorLight.TurnOn());
        // Sensor delay automation
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueInactive));
    }
}
