using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    MotionSensor motionSensor,
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<CookingAutomation> cookingAutomationLogger
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, lightAutomationLogger);

        yield return new CookingAutomation(cookingEntities, cookingAutomationLogger);
    }
}
