namespace HomeAutomation.apps.Common.Containers;

public interface IPantryMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    BinarySensorEntity RoomDoor { get; }
}

public class PantryMotionEntities(Entities entities) : IPantryMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.PantryMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.PantryMotionSensors;
    public LightEntity Light => entities.Light.PantryLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C63StillTargetDelay;
    public BinarySensorEntity MiScalePresenceSensor => entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity RoomDoor => entities.BinarySensor.ContactSensorDoor;
}
