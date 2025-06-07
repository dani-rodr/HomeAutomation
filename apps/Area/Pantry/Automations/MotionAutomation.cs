using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Pantry.Automations;

public class MotionAutomation(Entities entities, ILogger<Pantry> logger)
    : MotionAutomationBase(
        entities.Switch.PantryMotionSensor,
        entities.BinarySensor.PantryMotionSensors,
        entities.Light.PantryLights,
        entities.Number.ZEsp32C63StillTargetDelay,
        logger
    )
{
    protected override int SensorWaitTime => 10;
    private readonly BinarySensorEntity _miScalePresenceSensor = entities
        .BinarySensor
        .Esp32PresenceBedroomMiScalePresence;
    private readonly LightEntity _mirrorLight = entities.Light.ControllerRgbDf1c0d;
    private readonly BinarySensorEntity _roomDoor = entities.BinarySensor.ContactSensorDoor;

    public override void StartAutomation()
    {
        base.StartAutomation();
        _roomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch?.TurnOn());
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(_ =>
            {
                Light.TurnOff();
                _mirrorLight.TurnOff();
            });
        yield return _miScalePresenceSensor.StateChanges().IsOn().Subscribe(_ => _mirrorLight.TurnOn());
        // Sensor delay automation
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
    }
}
