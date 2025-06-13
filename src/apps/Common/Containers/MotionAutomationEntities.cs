namespace HomeAutomation.apps.Common.Containers;

public class BathroomMotionEntities(Entities entities) : IMotionAutomationEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BathroomPresenceSensors;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C62StillTargetDelay;
}
