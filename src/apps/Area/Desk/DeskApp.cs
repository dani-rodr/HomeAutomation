using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(IHaContext haContext, Entities entities, Services services, ILogger<DeskApp> logger)
    : AreaBase<DeskApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        IEventHandler eventHandler = new HaEventHandler(haContext, Logger);

        var lgDisplayEntities = new DeskLgDisplayEntities(Entities);
        var monitor = new LgDisplay(lgDisplayEntities, services, Logger);

        var desktopEntities = new DeskDesktopEntities(Entities);
        var destkop = new Desktop(desktopEntities, eventHandler, Logger);

        var laptop = new Laptop(eventHandler, Logger);

        var displayEntities = new DeskDisplayEntities(Entities);
        yield return new DisplayAutomations(displayEntities, monitor, destkop, laptop, Logger);
    }
}
