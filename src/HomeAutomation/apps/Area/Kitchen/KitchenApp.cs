using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Config;
using HomeAutomation.apps.Area.Kitchen.Devices;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Kitchen;

[AreaKey("kitchen")]
public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    IAreaConfigStore areaConfigStore,
    MotionSensor motionSensor,
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<CookingAutomation> cookingAutomationLogger
) : AppBase<KitchenApp, KitchenSettings>(areaConfigStore)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, Settings.Light, lightAutomationLogger);

        yield return new CookingAutomation(
            cookingEntities,
            Settings.Cooking,
            cookingAutomationLogger
        );
    }
}
