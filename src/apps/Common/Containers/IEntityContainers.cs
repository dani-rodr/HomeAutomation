namespace HomeAutomation.apps.Common.Containers;

public interface IMotionBase
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface ILightAutomationEntities : IMotionBase
{
    NumberEntity SensorDelay { get; }
    LightEntity Light { get; }
}

public interface IFanAutomationEntities : IMotionBase
{
    IEnumerable<SwitchEntity> Fans { get; }
}

public interface IBedroomLightEntities : ILightAutomationEntities
{
    SwitchEntity RightSideEmptySwitch { get; }
    SwitchEntity LeftSideFanSwitch { get; }
}

public interface ILivingRoomLightEntities : ILightAutomationEntities
{
    BinarySensorEntity BedroomDoor { get; }
    BinarySensorEntity BedroomMotionSensors { get; }
    MediaPlayerEntity TclTv { get; }
    BinarySensorEntity KitchenMotionSensors { get; }
    LightEntity PantryLights { get; }
    SwitchEntity PantryMotionSensor { get; }
    BinarySensorEntity PantryMotionSensors { get; }
}

public interface IPantryLightEntities : ILightAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    BinarySensorEntity BedroomDoor { get; }
}

public interface IBathroomLightEntities : ILightAutomationEntities;

public interface IDeskLightEntities : ILightAutomationEntities;

public interface IKitchenLightEntities : ILightAutomationEntities
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

public interface IClimateSchedulerEntities
{
    SensorEntity SunRising { get; }
    SensorEntity SunSetting { get; }
    SensorEntity SunMidnight { get; }
    WeatherEntity Weather { get; }
    InputBooleanEntity PowerSavingMode { get; }
}

public interface IClimateEntities : IMotionBase
{
    ClimateEntity AirConditioner { get; }
    BinarySensorEntity Door { get; }
    SwitchEntity FanAutomation { get; }
    BinarySensorEntity HouseMotionSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
    SwitchEntity Fan { get; }
}

public interface ILaptopEntities
{
    SwitchEntity VirtualSwitch { get; }
    ButtonEntity[] WakeOnLanButtons { get; }
    SensorEntity Session { get; }
    NumericSensorEntity BatteryLevel { get; }
    ButtonEntity Lock { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface IDisplayEntities
{
    MediaPlayerEntity MediaPlayer { get; }
}

public interface ITclDisplayEntities : IDisplayEntities, ILightAutomationEntities;

public interface ILgDisplayEntities : IDisplayEntities
{
    LightEntity Display { get; }
}

public interface ITabletEntities : ILightAutomationEntities
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
    SwitchEntity LivingRoomFanAutomation { get; }
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

public interface IChargingHandlerEntities
{
    NumericSensorEntity Level { get; }
    SwitchEntity Power { get; }
}
