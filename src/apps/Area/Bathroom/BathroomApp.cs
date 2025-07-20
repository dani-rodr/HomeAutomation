using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomLightEntities motionEntities,
    ILoggerFactory loggerFactory,
    MotionSensor motionSensor,
    IDimmingLightControllerFactory dimmingLightControllerFactory
) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay),
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
