namespace HomeAutomation.apps.Area.Desk.Devices;

public class DeskDevices(HomeAssistantGenerated.Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } =
        entities.BinarySensor.DeskMotionSensorSmartPresence;
    public ButtonEntity Restart { get; } = entities.Button.DeskMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.DeskMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.LgTvMotionSensor;
    public LightEntity Display { get; } = entities.Light.LgDisplay;
    public LightEntity SalaLights { get; } = entities.Light.SalaLights;
    public MediaPlayerEntity MediaPlayer { get; } = entities.MediaPlayer.LgWebosSmartTv;
    public SwitchEntity DesktopPower { get; } = entities.Switch.DanielPc;
    public InputButtonEntity RemotePcButton { get; } = entities.InputButton.RemotePc;
    public SwitchEntity LaptopVirtualSwitch { get; } = entities.Switch.Laptop;
    public SwitchEntity LaptopPowerPlug { get; } = entities.Switch.Sonoff1002380fe51;
    public ButtonEntity LaptopWakeOnLanButton { get; } = entities.Button.Thinkpadt14WakeOnWlan;
    public SensorEntity LaptopSession { get; } = entities.Sensor.Thinkpadt14Sessionstate;
    public NumericSensorEntity LaptopBatteryLevel { get; } =
        entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;
    public ButtonEntity LaptopLock { get; } = entities.Button.Thinkpadt14Lock;
    public ButtonEntity LaptopSleep { get; } = entities.Button.Thinkpadt14Sleep;
    public InputBooleanEntity ProjectNationWeek { get; } = entities.InputBoolean.ProjectNationWeek;
}
