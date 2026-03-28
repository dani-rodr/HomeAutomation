using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Services.Schedulers.Entities;

public class LaptopSchedulerEntities(DeskDevices devices) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => devices.ProjectNationWeek;
}
