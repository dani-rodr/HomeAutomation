namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomLightDevices(Entities entities)
{
    public SwitchEntity LightAutomation { get; } = entities.Switch.SalaMotionSensor;
    public BinarySensorEntity MotionSensor { get; } =
        entities.BinarySensor.LivingRoomPresenceSensors;
    public LightEntity Lights { get; } = entities.Light.SalaLightsGroup;
    public NumberEntity SensorDelay { get; } = entities.Number.SalaMotionSensorStillTargetDelay;
    public ButtonEntity Restart { get; } = entities.Button.SalaMotionSensorRestartEsp32;
    public BinarySensorEntity BedroomDoor { get; } = entities.BinarySensor.ContactSensorDoor;
    public BinarySensorEntity LivingRoomDoor { get; } = entities.BinarySensor.DoorWrapper;
    public BinarySensorEntity BedroomMotionSensor { get; } =
        entities.BinarySensor.BedroomPresenceSensors;
    public BinarySensorEntity KitchenMotionSensor { get; } =
        entities.BinarySensor.KitchenMotionSensors;
    public SwitchEntity KitchenMotionAutomation { get; } = entities.Switch.KitchenMotionSensor;
    public LightEntity PantryLights { get; } = entities.Light.PantryLights;
    public SwitchEntity PantryMotionAutomation { get; } = entities.Switch.PantryMotionSensor;
    public BinarySensorEntity PantryMotionSensor { get; } =
        entities.BinarySensor.PantryMotionSensors;
}
