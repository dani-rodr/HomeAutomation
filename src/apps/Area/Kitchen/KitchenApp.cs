using HomeAutomation.apps.Area.Kitchen.Automations;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenMotionEntities motionEntities,
    ICookingEntities cookingEntities,
    ILogger<KitchenApp> logger
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(motionEntities, logger);
        yield return new CookingAutomation(cookingEntities, logger);
    }
}
