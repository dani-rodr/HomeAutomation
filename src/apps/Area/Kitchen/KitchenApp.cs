using HomeAutomation.apps.Area.Kitchen.Automations;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenMotionEntities motionEntities,
    ICookingEntities cookingEntities,
    ILoggerFactory loggerFactory
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
        yield return new CookingAutomation(
            cookingEntities,
            loggerFactory.CreateLogger<CookingAutomation>()
        );
    }
}
