using System.Collections.Generic;
using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(Entities entities, ILogger<BathroomApp> logger) : AreaBase<BathroomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var motionEntities = new BathroomMotionEntities(Entities);
        yield return new MotionAutomation(motionEntities, Logger);
    }
}
