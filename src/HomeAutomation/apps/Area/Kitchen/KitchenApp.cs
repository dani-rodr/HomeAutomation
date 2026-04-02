using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Config;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    IAppConfig<KitchenSettings> settings,
    MotionSensor motionSensor,
    IAutomationFactory automationFactory
) : AppBase<KitchenSettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(motionEntities, Settings.Light);

        yield return automationFactory.Create<CookingAutomation>(cookingEntities, Settings.Cooking);
    }
}
