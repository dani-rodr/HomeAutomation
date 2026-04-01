using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;

public class GlobalClimateSchedulerEntities(GlobalDevices devices) : IClimateSchedulerEntities
{
    public SensorEntity SunRising => devices.SunRising;
    public SensorEntity SunSetting => devices.SunSetting;
    public SensorEntity SunMidnight => devices.SunMidnight;
    public InputBooleanEntity PowerSavingMode => devices.PowerSavingMode;
}
