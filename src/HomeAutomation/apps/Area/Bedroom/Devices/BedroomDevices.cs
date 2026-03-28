namespace HomeAutomation.apps.Area.Bedroom.Devices;

public class BedroomDevices(Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } = entities.BinarySensor.BedroomPresenceSensors;
    public ButtonEntity Restart { get; } = entities.Button.BedroomMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.BedroomMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.BedroomMotionSensor;
    public LightEntity Lights { get; } = entities.Light.BedLights;
    public SwitchEntity FanAutomation { get; } = entities.Switch.BedroomFanAutomation;
    public SwitchEntity MainFan { get; } = entities.Switch.Sonoff100238104e1;
    public BinarySensorEntity Door { get; } = entities.BinarySensor.ContactSensorDoor;
    public SwitchEntity ClimateAutomation { get; } = entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner { get; } = entities.Climate.Ac;
    public ButtonEntity AcFanModeToggle { get; } = entities.Button.AcFanModeToggle;
    public InputBooleanEntity PowerSavingMode { get; } = entities.InputBoolean.AcPowerSavingMode;
    public SwitchEntity RightSideEmptySwitch { get; } = entities.Switch.Sonoff1002352c401;
}
