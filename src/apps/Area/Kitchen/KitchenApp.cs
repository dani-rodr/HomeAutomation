using HomeAutomation.apps.Area.Kitchen.Automations;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    IScheduler scheduler,
    ILoggerFactory loggerFactory
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new LightAutomation(
            motionEntities,
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new CookingAutomation(
            cookingEntities,
            loggerFactory.CreateLogger<CookingAutomation>()
        );
    }
}
