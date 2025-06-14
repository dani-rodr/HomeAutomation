using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(Entities entities, Services services, IEventHandler eventHandler, ILogger<DeskApp> logger)
    : AreaBase<DeskApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var lgDisplayEntities = new DeskLgDisplayEntities(Entities);
        var monitor = new LgDisplay(lgDisplayEntities, services, Logger);

        var desktopEntities = new DeskDesktopEntities(Entities);
        var desktop = new Desktop(desktopEntities, eventHandler, new NotificationServices(services, logger), Logger);

        var laptopEntities = new LaptopEntities(Entities);
        var laptop = new Laptop(laptopEntities, eventHandler, Logger);

        var displayEntities = new DeskDisplayEntities(Entities);
        yield return new DisplayAutomations(displayEntities, monitor, desktop, laptop, eventHandler, Logger);
    }
}
