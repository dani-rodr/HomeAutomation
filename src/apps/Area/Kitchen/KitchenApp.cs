using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    MotionSensor motionSensor,
    IAirFryer airFryer,
    ILoggerFactory loggerFactory
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;
        yield return airFryer;
        yield return new LightAutomation(
            motionEntities,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new CookingAutomation(
            cookingEntities,
            loggerFactory.CreateLogger<CookingAutomation>()
        );
    }
}
