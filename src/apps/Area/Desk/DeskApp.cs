using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(
    Services services,
    IEventHandler eventHandler,
    INotificationServices notificationServices,
    ILgDisplayEntities lgDisplayEntities,
    IDesktopEntities desktopEntities,
    ILaptopEntities laptopEntities,
    IDisplayEntities displayEntities,
    ILogger<DeskApp> logger
) : AppBase<DeskApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        LgDisplay monitor = new(lgDisplayEntities, services, logger);
        Desktop desktop = new(desktopEntities, eventHandler, notificationServices, logger);
        Laptop laptop = new(laptopEntities, eventHandler, logger);
        // yield return new MotionAutomation(deskMotionEntities, dimmingController, logger);
        yield return new DisplayAutomations(displayEntities, monitor, desktop, laptop, eventHandler, logger);
    }
}
