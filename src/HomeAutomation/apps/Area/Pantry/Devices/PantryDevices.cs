namespace HomeAutomation.apps.Area.Pantry.Devices;

public class PantryDevices(Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } = entities.BinarySensor.PantryMotionSensors;
    public ButtonEntity Restart { get; } = entities.Button.PantryMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.PantryMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.PantryMotionSensor;
    public LightEntity Lights { get; } = entities.Light.PantryLights;
    public BinarySensorEntity MiScalePresenceSensor { get; } =
        entities.BinarySensor.BedroomMotionSensorMiScalePresence;
    public LightEntity MirrorLight { get; } = entities.Light.ControllerRgbDf1c0d;
    public SwitchEntity BathroomMotionAutomation { get; } = entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity BathroomMotionSensor { get; } =
        entities.BinarySensor.BathroomPresenceSensors;
}
