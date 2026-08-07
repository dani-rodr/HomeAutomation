namespace HomeAutomation.apps.Common.Devices;

public class GlobalDevices(Entities entities)
{
    public BinarySensorEntity HouseMotionSensor { get; } = entities.BinarySensor.House;
    public ButtonEntity Restart { get; } = entities.Button.RestartEsp32;
    public NumberEntity MotionSensorDelay { get; } =
        entities.Number.SalaMotionSensorStillTargetDelay;
    public SwitchEntity MotionSensorAutomation { get; } = entities.Switch.MotionSensors;
    public LightEntity Lights { get; } = entities.Light.Lights;
    public SensorEntity SunRising { get; } = entities.Sensor.SunNextRising;
    public SensorEntity SunSetting { get; } = entities.Sensor.SunNextSetting;
    public SensorEntity SunMidnight { get; } = entities.Sensor.SunNextMidnight;
    public WeatherEntity Weather { get; } = entities.Weather.Home;
    public InputBooleanEntity PowerSavingMode { get; } = entities.InputBoolean.AcPowerSavingMode;
    public InputBooleanEntity DanielPresence { get; } = entities.InputBoolean.DanielHome;
    public ButtonEntity DanielToggle { get; } = entities.Button.ManualTrackerButtonDaniel;
    public InputBooleanEntity AthenaPresence { get; } = entities.InputBoolean.AthenaHome;
    public ButtonEntity AthenaToggle { get; } = entities.Button.ManualTrackerButtonAthena;
    public CounterEntity PeopleCounter { get; } = entities.Counter.People;
}
