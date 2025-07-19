using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    IScheduler scheduler,
    MotionSensor motionSensor,
    ILoggerFactory loggerFactory
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;
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
