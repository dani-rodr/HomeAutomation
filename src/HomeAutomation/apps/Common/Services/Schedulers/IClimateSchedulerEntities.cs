namespace HomeAutomation.apps.Common.Services.Schedulers;

public interface IClimateSchedulerEntities
{
    SensorEntity SunRising { get; }
    SensorEntity SunSetting { get; }
    SensorEntity SunMidnight { get; }
    WeatherEntity Weather { get; }
    InputBooleanEntity PowerSavingMode { get; }
}
