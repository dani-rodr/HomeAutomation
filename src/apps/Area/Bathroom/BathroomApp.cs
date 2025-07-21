using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomLightEntities motionEntities,
    ILoggerFactory loggerFactory,
    Devices.MotionSensor motionSensor,
    IDimmingLightControllerFactory dimmingLightControllerFactory
) : AppBase<BathroomApp>()
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
            dimmingLightControllerFactory.Create(motionSensor.SensorDelay),
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
