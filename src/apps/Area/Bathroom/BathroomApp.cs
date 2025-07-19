using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomLightEntities motionEntities,
    ITypedEntityFactory entityFactory,
    ILoggerFactory loggerFactory,
    IScheduler scheduler
) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionSensor(entityFactory, loggerFactory.CreateLogger<MotionSensor>());

        yield return new LightAutomation(
            motionEntities,
            new DimmingLightController(
                motionEntities.SensorDelay,
                scheduler,
                loggerFactory.CreateLogger<DimmingLightController>()
            ),
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
