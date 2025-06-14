using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILogger<BedroomApp> logger,
    IBedroomMotionEntities motionEntities,
    IFanAutomationEntities fanEntities,
    IClimateAutomationEntities climateEntities,
    IScheduler scheduler
) : AreaBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(motionEntities, logger);
        yield return new FanAutomation(fanEntities, logger);
        yield return new ClimateAutomation(climateEntities, scheduler, logger);
    }
}
