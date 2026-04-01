using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;

public class GlobalClimateSchedulerEntities(GlobalDevices devices) : IClimateSchedulerEntities
{
    public InputBooleanEntity PowerSavingMode => devices.PowerSavingMode;
}
