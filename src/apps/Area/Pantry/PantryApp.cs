using System.Collections.Generic;
using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(Entities entities, ILogger<PantryApp> logger) : AreaBase<PantryApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations() => [new MotionAutomation(Entities, Logger)];
}
