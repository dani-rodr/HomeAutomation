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
        var eventHandler = new HaEventHandler(haContext, Logger);
        var monitor = new LgDisplay(Entities, services, Logger);
        var destkop = new Desktop(Entities, eventHandler, Logger);
        yield return new DisplayAutomations(Entities, monitor, destkop, Logger);
    }
}
