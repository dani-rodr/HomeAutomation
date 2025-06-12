using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(Entities entities, Services services, ILogger<DeskApp> logger)
    : AreaBase<DeskApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var monitor = new LgDisplay(Entities, services, logger);
        yield return new DisplayAutomations(Entities, monitor, Logger);
    }
}
