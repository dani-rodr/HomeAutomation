using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomMotionEntities motionEntities,
    ILoggerFactory loggerFactory,
    IScheduler scheduler
) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            new DimmingLightController(
                motionEntities.SensorDelay,
                scheduler,
                loggerFactory.CreateLogger<DimmingLightController>()
            ),
            loggerFactory.CreateLogger<MotionAutomation>()
        );
    }
}
