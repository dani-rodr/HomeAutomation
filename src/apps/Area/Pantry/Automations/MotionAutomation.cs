using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Pantry.Automations;

public class MotionAutomation(Entities entities, ILogger logger)
    : MotionAutomationBase(
        entities.Switch.PantryMotionSensor,
        entities.BinarySensor.PantryMotionSensors,
        entities.Light.PantryLights,
        logger,
        entities.Number.ZEsp32C63StillTargetDelay
    )
{
    protected override int SensorWaitTime => 10;
    private readonly BinarySensorEntity _miScalePresenceSensor = entities
        .BinarySensor
        .Esp32PresenceBedroomMiScalePresence;
    private readonly LightEntity _mirrorLight = entities.Light.ControllerRgbDf1c0d;
    private readonly BinarySensorEntity _roomDoor = entities.BinarySensor.ContactSensorDoor;

    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations() =>
        [_roomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch?.TurnOn())];

    protected override IEnumerable<IDisposable> GetLightAutomations()
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
    }
}
