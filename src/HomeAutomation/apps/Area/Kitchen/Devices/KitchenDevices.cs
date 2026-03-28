namespace HomeAutomation.apps.Area.Kitchen.Devices;

public class KitchenDevices(Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } = entities.BinarySensor.KitchenMotionSensors;
    public ButtonEntity Restart { get; } = entities.Button.KitchenMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.KitchenMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.KitchenMotionSensor;
    public LightEntity Lights { get; } = entities.Light.RgbLightStrip;
    public BinarySensorEntity PowerPlug { get; } =
        entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
    public NumericSensorEntity RiceCookerPower { get; } = entities.Sensor.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch { get; } = entities.Switch.RiceCookerSocket1;
    public SensorEntity AirFryerStatus { get; } =
        entities.Sensor.XiaomiSmartAirFryerPro4lAirFryerOperatingStatus;
    public ButtonEntity InductionTurnOff { get; } = entities.Button.InductionCookerPower;
    public NumericSensorEntity InductionPower { get; } = entities.Sensor.SmartPlug3SonoffS31Power;
    public SwitchEntity CookingAutomation { get; } = entities.Switch.CookingAutomation;
    public TimerEntity AirFryerTimer { get; } = entities.Timer.AirFryer;
}
