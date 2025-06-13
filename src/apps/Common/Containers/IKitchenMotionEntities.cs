namespace HomeAutomation.apps.Common.Containers;

public interface IKitchenMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity PowerPlug { get; }
}

public class KitchenMotionEntities(Entities entities) : IKitchenMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.KitchenMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.KitchenMotionSensors;
    public LightEntity Light => entities.Light.RgbLightStrip;
    public NumberEntity SensorDelay => entities.Number.Ld2410Esp325StillTargetDelay;
    public BinarySensorEntity PowerPlug => entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
}