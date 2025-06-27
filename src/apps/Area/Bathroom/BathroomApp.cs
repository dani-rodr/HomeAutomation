using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomMotionEntities motionEntities,
    ILogger<BathroomApp> logger,
    IScheduler scheduler,
    ILogger<DimmingLightController> dimmingLogger
) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            new DimmingLightController(motionEntities.SensorDelay, scheduler, dimmingLogger),
            logger
        );
    }
}
