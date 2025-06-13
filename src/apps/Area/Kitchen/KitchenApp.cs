using System.Collections.Generic;
using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(Entities entities, ILogger<KitchenApp> logger) : AreaBase<KitchenApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var motionEntities = new KitchenMotionEntities(Entities);
        yield return new MotionAutomation(motionEntities, Logger);
        
        var cookingEntities = new KitchenCookingEntities(Entities);
        yield return new CookingAutomation(cookingEntities, Logger);
    }
}
