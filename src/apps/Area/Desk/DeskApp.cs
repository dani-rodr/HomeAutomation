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
    IDeskLightEntities deskMotionEntities,
    ILgDisplay lgDisplay,
    IScheduler scheduler,
    ILoggerFactory loggerFactory
) : AppBase<DeskApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        Desktop desktop = new(
            desktopEntities,
            eventHandler,
            notificationServices,
            scheduler,
            loggerFactory.CreateLogger<Desktop>()
        );
        Laptop laptop = new(
            laptopEntities,
            laptopScheduler,
            laptopBatteryHandler,
            eventHandler,
            loggerFactory.CreateLogger<Laptop>()
        );
        desktop.StartAutomation();
        laptop.StartAutomation();
        lgDisplay.StartAutomation();
        yield return new LightAutomation(
            deskMotionEntities,
            lgDisplay,
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new DisplayAutomation(
            lgDisplay,
            desktop,
            laptop,
            eventHandler,
            loggerFactory.CreateLogger<DisplayAutomation>()
        );
    }
}
