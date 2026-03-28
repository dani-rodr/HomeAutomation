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
