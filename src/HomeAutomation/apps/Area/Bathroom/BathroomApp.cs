using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Automations.Entities;
using HomeAutomation.apps.Area.Bathroom.Config;
using HomeAutomation.apps.Area.Bathroom.Devices;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Bathroom;

[AreaKey("bathroom")]
public class BathroomApp(
    IBathroomLightEntities motionEntities,
    ILogger<LightAutomation> lightAutomationLogger,
    MotionSensor motionSensor,
    IAreaConfigStore areaConfigStore,
    IDimmingLightControllerFactory dimmingLightControllerFactory
) : AppBase<BathroomApp, BathroomSettings>(areaConfigStore)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            Settings.Light,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay),
            lightAutomationLogger
        );
    }
}
