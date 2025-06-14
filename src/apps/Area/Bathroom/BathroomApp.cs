using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(IBathroomMotionEntities motionEntities, ILogger<BathroomApp> logger) : AreaBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var dimmingController = new DimmingLightController(motionEntities.SensorDelay);

        yield return new MotionAutomation(motionEntities, dimmingController, logger);
    }
}
