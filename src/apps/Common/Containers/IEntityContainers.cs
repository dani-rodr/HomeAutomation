namespace HomeAutomation.apps.Common.Containers;

public interface IMotionBase
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface IMotionAutomationEntities : IMotionBase
{
    NumberEntity SensorDelay { get; }
    LightEntity Light { get; }
}

public interface IFanAutomationEntities : IMotionBase
{
    IEnumerable<SwitchEntity> Fans { get; }
}

public interface IBedroomMotionEntities : IMotionAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}

public interface ILivingRoomMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity BedroomDoor { get; }
    BinarySensorEntity BedroomMotionSensors { get; }
    MediaPlayerEntity TclTv { get; }
    BinarySensorEntity KitchenMotionSensors { get; }
    LightEntity PantryLights { get; }
    SwitchEntity PantryMotionSensor { get; }
    BinarySensorEntity PantryMotionSensors { get; }
}

public interface IPantryMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    BinarySensorEntity BedroomDoor { get; }
}

public interface IBathroomMotionEntities : IMotionAutomationEntities;

public interface IDeskMotionEntities : IMotionAutomationEntities;

public interface IKitchenMotionEntities : IMotionAutomationEntities
{
    BinarySensorEntity PowerPlug { get; }
}

public interface ICookingEntities
{
    NumericSensorEntity RiceCookerPower { get; }
    SwitchEntity RiceCookerSwitch { get; }
    SensorEntity AirFryerStatus { get; }
    ButtonEntity InductionTurnOff { get; }
    NumericSensorEntity InductionPower { get; }
}

public interface IWeatherSensor
{
    SensorEntity SunRising { get; }
    SensorEntity SunSetting { get; }
    SensorEntity SunMidnight { get; }
    WeatherEntity Weather { get; }
}

public interface IClimateEntities : IWeatherSensor, IMotionBase
{
    ClimateEntity AirConditioner { get; }
    BinarySensorEntity Door { get; }
    SwitchEntity FanAutomation { get; }
    InputBooleanEntity PowerSavingMode { get; }
    BinarySensorEntity HouseMotionSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
}

public interface ILaptopEntities
{
    SwitchEntity VirtualSwitch { get; }
    ButtonEntity[] WakeOnLanButtons { get; }
    SensorEntity Session { get; }
    NumericSensorEntity BatteryLevel { get; }
    ButtonEntity Lock { get; }
}

public interface IDisplayEntities
{
    MediaPlayerEntity MediaPlayer { get; }
}

public interface ITclDisplayEntities : IDisplayEntities;

public interface ILgDisplayEntities : IDisplayEntities
{
    LightEntity Display { get; }
}

public interface ITabletEntities : IMotionAutomationEntities
{
    BinarySensorEntity TabletActive { get; }
}

public interface IBedroomFanEntities : IFanAutomationEntities;

public interface ILivingRoomFanEntities : IFanAutomationEntities
{
    BinarySensorEntity BedroomMotionSensor { get; }
}

public interface IAirQualityEntities : IFanAutomationEntities
{
    NumericSensorEntity Pm25Sensor { get; }
    SwitchEntity LedStatus { get; }
    SwitchEntity LivingRoomSwitch { get; }
}

public interface IDesktopEntities
{
    SwitchEntity Power { get; }
    InputButtonEntity RemotePcButton { get; }
}

public interface ILockingEntities : IMotionBase
{
    LockEntity Lock { get; }
    BinarySensorEntity Door { get; }
    BinarySensorEntity HouseStatus { get; }
    SwitchEntity Flytrap { get; }
}

public interface ILaptopSchedulerEntities
{
    InputBooleanEntity ProjectNationWeek { get; }
}

public interface IBatteryHandlerEntities
{
    NumericSensorEntity Level { get; }
    SwitchEntity Power { get; }
}
