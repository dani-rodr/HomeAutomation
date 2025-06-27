using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILogger<BedroomApp> logger,
    IBedroomMotionEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IScheduler scheduler
) : AppBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(motionEntities, scheduler, logger);
        yield return new FanAutomation(fanEntities, logger);
        yield return new ClimateAutomation(climateEntities, scheduler, logger);
    }
}
