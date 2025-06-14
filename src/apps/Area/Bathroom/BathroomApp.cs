using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(IBathroomMotionEntities motionEntities, ILogger<BathroomApp> logger) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            new DimmingLightController(motionEntities.SensorDelay),
            logger
        );
    }
}
