namespace HomeAutomation.apps.Area.Bathroom.Devices;

public class BathroomDevices(Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } = entities.BinarySensor.BathroomPresenceSensors;
    public ButtonEntity Restart { get; } = entities.Button.BathroomMotionSensorRestart;
    public NumberEntity SensorDelay { get; } = entities.Number.BathroomMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.BathroomMotionSensor;
    public LightEntity Lights { get; } = entities.Light.BathroomLights;
}
