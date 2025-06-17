namespace HomeAutomation.apps.Common.Containers;

public interface IMotionBase
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface IMotionWithLight : IMotionBase
{
    LightEntity Light { get; }
}

public interface IMotionWithLightAndDelay : IMotionWithLight
{
    NumberEntity SensorDelay { get; }
}

public interface IMotionAutomationEntities : IMotionWithLightAndDelay;

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

public interface IAirQualityEntities : IMotionBase
{
    SwitchEntity AirPurifierFan { get; }
    SwitchEntity SupportingFan { get; }
    NumericSensorEntity Pm25Sensor { get; }
    SwitchEntity LedStatus { get; }
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
    SwitchEntity FanSwitch { get; }
    InputBooleanEntity PowerSavingMode { get; }
    BinarySensorEntity HouseMotionSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
}

public interface ILaptopEntities
{
    SwitchEntity VirtualSwitch { get; }
    ButtonEntity[] WakeOnLanButtons { get; }
    SwitchEntity PowerPlug { get; }
    SensorEntity Session { get; }
    NumericSensorEntity BatteryLevel { get; }
    ButtonEntity Lock { get; }
}

public interface ILgDisplayEntities
{
    MediaPlayerEntity MediaPlayer { get; }
    SwitchEntity Screen { get; }
    InputNumberEntity Brightness { get; }
}

public interface ITabletEntities : IMotionBase
{
    LightEntity TabletScreen { get; }
    BinarySensorEntity TabletActive { get; }
}

public interface IFanEntities : IMotionBase
{
    IEnumerable<SwitchEntity> Fans { get; }
}

public interface ILivingRoomFanEntities : IFanEntities
{
    BinarySensorEntity BedroomMotionSensor { get; }
}

public interface IDesktopEntities
{
    BinarySensorEntity PowerPlugThreshold { get; }
    BinarySensorEntity NetworkStatus { get; }
    SwitchEntity PowerSwitch { get; }
    InputButtonEntity RemotePcButton { get; }
}

public interface ILockingEntities : IMotionBase
{
    LockEntity Lock { get; }
    BinarySensorEntity Door { get; }
    BinarySensorEntity HouseStatus { get; }
    SwitchEntity Flytrap { get; }
}
