using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(
    IEventHandler eventHandler,
    INotificationServices notificationServices,
    IDesktopEntities desktopEntities,
    ILaptopEntities laptopEntities,
    ILaptopScheduler laptopScheduler,
    IBatteryHandler laptopBatteryHandler,
    IDeskMotionEntities deskMotionEntities,
    ILgDisplay lgDisplay,
    ILogger<DeskApp> logger
) : AppBase<DeskApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        Desktop desktop = new(desktopEntities, eventHandler, notificationServices, logger);
        Laptop laptop = new(laptopEntities, laptopScheduler, laptopBatteryHandler, eventHandler, logger);
        yield return new MotionAutomation(deskMotionEntities, lgDisplay, logger);
        yield return new DisplayAutomations(lgDisplay, desktop, laptop, eventHandler, logger);
    }
}
