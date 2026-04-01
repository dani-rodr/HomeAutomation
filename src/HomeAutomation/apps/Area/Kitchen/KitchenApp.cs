using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Config;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

[AreaKey("kitchen")]
public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    IAppConfig<KitchenSettings> settings,
    MotionSensor motionSensor,
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<CookingAutomation> cookingAutomationLogger
) : AppBase<KitchenApp, KitchenSettings>(settings.Value)
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
