using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Automations.Entities;
using HomeAutomation.apps.Area.Bathroom.Config;
using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomLightEntities motionEntities,
    MotionSensor motionSensor,
    IAppConfig<BathroomSettings> settings,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    IAutomationFactory automationFactory
) : AppBase<BathroomSettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(
            motionEntities,
            Settings.Light,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay)
        );
    }
}
