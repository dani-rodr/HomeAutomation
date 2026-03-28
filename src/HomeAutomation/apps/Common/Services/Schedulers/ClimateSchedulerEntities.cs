using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Common.Services.Schedulers;

public class GlobalClimateSchedulerEntities(GlobalDevices devices) : IClimateSchedulerEntities
{
    public SensorEntity SunRising => devices.SunRising;
    public SensorEntity SunSetting => devices.SunSetting;
    public SensorEntity SunMidnight => devices.SunMidnight;
    public WeatherEntity Weather => devices.Weather;
    public InputBooleanEntity PowerSavingMode => devices.PowerSavingMode;
}
