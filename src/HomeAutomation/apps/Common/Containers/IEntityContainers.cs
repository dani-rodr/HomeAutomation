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
