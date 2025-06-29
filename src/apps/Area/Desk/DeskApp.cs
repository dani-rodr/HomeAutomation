using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(
    IEventHandler eventHandler,
    INotificationServices notificationServices,
    IDesktopEntities desktopEntities,
    ILaptopEntities laptopEntities,
    ILaptopScheduler laptopScheduler,
    ILaptopChargingHandler laptopBatteryHandler,
    IDeskMotionEntities deskMotionEntities,
    ILgDisplay lgDisplay,
    IScheduler scheduler,
    ILogger<DeskApp> logger
) : AppBase<DeskApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        Desktop desktop = new(
            desktopEntities,
            eventHandler,
            notificationServices,
            scheduler,
            logger
        );
        Laptop laptop = new(
            laptopEntities,
            laptopScheduler,
            laptopBatteryHandler,
            eventHandler,
            logger
        );
        desktop.StartAutomation();
        laptop.StartAutomation();
        lgDisplay.StartAutomation();
        yield return new MotionAutomation(deskMotionEntities, lgDisplay, logger);
        yield return new DisplayAutomation(lgDisplay, desktop, laptop, eventHandler, logger);
    }
}
