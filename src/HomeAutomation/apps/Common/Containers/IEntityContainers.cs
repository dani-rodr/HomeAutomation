namespace HomeAutomation.apps.Common.Containers;

public interface IPersonEntities
{
    PersonEntity Person { get; }
    ButtonEntity ToggleLocation { get; }
    CounterEntity Counter { get; }
    IEnumerable<BinarySensorEntity> HomeTriggers { get; }
    IEnumerable<BinarySensorEntity> AwayTriggers { get; }
    IEnumerable<BinarySensorEntity> DirectUnlockTriggers { get; }
}

public interface IMotionBase
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public interface ILightAutomationEntities : IMotionBase
{
    NumberEntity SensorDelay { get; }
    LightEntity Light { get; }
    ButtonEntity Restart { get; }
}

public interface IFanAutomationEntities : IMotionBase
{
    IEnumerable<SwitchEntity> Fans { get; }
}

public interface ILivingRoomLightEntities : ILightAutomationEntities
{
    BinarySensorEntity BedroomDoor { get; }
    BinarySensorEntity LivingRoomDoor { get; }
    BinarySensorEntity BedroomMotionSensor { get; }
    MediaPlayerEntity TclTv { get; }
    BinarySensorEntity KitchenMotionSensor { get; }
    SwitchEntity KitchenMotionAutomation { get; }
    LightEntity PantryLights { get; }
    SwitchEntity PantryMotionAutomation { get; }
    BinarySensorEntity PantryMotionSensor { get; }
}

public interface IPantryLightEntities : ILightAutomationEntities
{
    BinarySensorEntity MiScalePresenceSensor { get; }
    LightEntity MirrorLight { get; }
    SwitchEntity BathroomMotionAutomation { get; }
    BinarySensorEntity BathroomMotionSensor { get; }
}

public interface IClimateSchedulerEntities
{
    SensorEntity SunRising { get; }
    SensorEntity SunSetting { get; }
    SensorEntity SunMidnight { get; }
    WeatherEntity Weather { get; }
    InputBooleanEntity PowerSavingMode { get; }
}

public interface IDisplayEntities
{
    MediaPlayerEntity MediaPlayer { get; }
}

public interface ITclDisplayEntities : IDisplayEntities, ILightAutomationEntities;

public interface ITabletEntities : ILightAutomationEntities
{
    BinarySensorEntity TabletActive { get; }
}

public interface ILivingRoomFanEntities : IFanAutomationEntities
{
    BinarySensorEntity BedroomMotionSensor { get; }
    SwitchEntity ExhaustFan { get; }
}

public interface IAirQualityEntities : IFanAutomationEntities
{
    NumericSensorEntity Pm25Sensor { get; }
    SwitchEntity LedStatus { get; }
    SwitchEntity LivingRoomFanAutomation { get; }
    SwitchEntity SupportingFan { get; }
}

public interface ILockingEntities : IMotionBase
{
    LockEntity Lock { get; }
    BinarySensorEntity Door { get; }
    BinarySensorEntity HouseStatus { get; }
    SwitchEntity Flytrap { get; }
}

public interface IAccessControlAutomationEntities
{
    BinarySensorEntity Door { get; }
    BinarySensorEntity House { get; }
    LockEntity Lock { get; }
}
