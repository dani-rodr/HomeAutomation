using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen;

public class KitchenApp(
    IKitchenLightEntities motionEntities,
    ICookingEntities cookingEntities,
    Devices.MotionSensor motionSensor,
    ILoggerFactory loggerFactory
) : AppBase<KitchenApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        var motionAutomation = new MotionAutomation(
            motionSensor,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
        yield return motionAutomation;

        yield return new LightAutomation(
            motionEntities,
            motionAutomation,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new CookingAutomation(
            cookingEntities,
            loggerFactory.CreateLogger<CookingAutomation>()
        );
    }
}
