namespace HomeAutomation.apps.Common.Containers;

public interface IBedroomMotionEntities : IMotionAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}

public class BedroomMotionEntities(Entities entities) : IBedroomMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BedroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BedroomPresenceSensors;
    public LightEntity Light => entities.Light.BedLights;
    public NumberEntity SensorDelay => entities.Number.Esp32PresenceBedroomStillTargetDelay;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => entities.Switch.Sonoff100238104e1;
}
