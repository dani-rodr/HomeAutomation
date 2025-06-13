using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(Entities entities, ILogger<BathroomApp> logger) : AreaBase<BathroomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var motionEntities = new BathroomMotionEntities(Entities);
        var dimmingController = new DimmingLightController(motionEntities.SensorDelay);

        yield return new MotionAutomation(motionEntities, dimmingController, Logger);
    }
}
