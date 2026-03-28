namespace HomeAutomation.apps.Area.Desk.Devices;

public class LaptopSchedulerEntities(DeskDevices devices) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => devices.ProjectNationWeek;
}
