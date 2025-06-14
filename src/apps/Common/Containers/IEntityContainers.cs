namespace HomeAutomation.apps.Common.Containers;

public interface IMotionBase
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface IMotionWithLight : IMotionBase
{
    LightEntity Light { get; }
    NumberEntity SensorDelay { get; }
}

public interface IMotionAutomationEntities : IMotionWithLight;

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

public interface ICookingAutomationEntities
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

public interface IClimateAutomationEntities : IWeatherSensor, IMotionBase
{
    ClimateEntity AirConditioner { get; }
    BinarySensorEntity Door { get; }
    SwitchEntity FanSwitch { get; }
    InputBooleanEntity PowerSavingMode { get; }
    BinarySensorEntity HouseSensor { get; }
    ButtonEntity AcFanModeToggle { get; }
}

public interface IDisplayAutomationEntities
{
    SwitchEntity LgScreen { get; }
    InputNumberEntity LgTvBrightness { get; }
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
    MediaPlayerEntity LgWebosSmartTv { get; }
}

public interface ITabletAutomationEntities : IMotionBase
{
    LightEntity TabletScreen { get; }
    BinarySensorEntity TabletActive { get; }
}

public interface IFanAutomationEntities : IMotionBase
{
    IEnumerable<SwitchEntity> Fans { get; }
}

public interface ILivingRoomFanEntities : IFanAutomationEntities
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
