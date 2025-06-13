namespace HomeAutomation.apps.Common.Containers;

public interface IMotionAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    LightEntity Light { get; }
    NumberEntity SensorDelay { get; }
}
