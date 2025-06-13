namespace HomeAutomation.apps.Common.Containers;

public interface IClimateAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    ClimateEntity AirConditioner { get; }
    BinarySensorEntity MotionSensor { get; }
    BinarySensorEntity DoorSensor { get; }
    SwitchEntity FanSwitch { get; }
    InputBooleanEntity PowerSavingMode { get; }
    SensorEntity SunRising { get; }
    SensorEntity SunSetting { get; }
    SensorEntity SunMidnight { get; }
    BinarySensorEntity HouseSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
    WeatherEntity Weather { get; }
}

public class BedroomClimateEntities(Entities entities) : IClimateAutomationEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner => entities.Climate.Ac;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BedroomPresenceSensors;
    public BinarySensorEntity DoorSensor => entities.BinarySensor.ContactSensorDoor;
    public SwitchEntity FanSwitch => entities.Switch.Sonoff100238104e1;
    public InputBooleanEntity PowerSavingMode => entities.InputBoolean.AcPowerSavingMode;
    public SensorEntity SunRising => entities.Sensor.SunNextRising;
    public SensorEntity SunSetting => entities.Sensor.SunNextSetting;
    public SensorEntity SunMidnight => entities.Sensor.SunNextMidnight;
    public BinarySensorEntity HouseSensor => entities.BinarySensor.House;
    public ButtonEntity AcFanModeToggle => entities.Button.AcFanModeToggle;
    public WeatherEntity Weather => entities.Weather.Home;
}