using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(Entities entities, ILogger<BedroomApp> logger, IScheduler scheduler)
    : AreaBase<BedroomApp>(entities, logger, scheduler)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var motionEntities = new BedroomMotionEntities(Entities);
        yield return new MotionAutomation(motionEntities, Logger);

        var fanEntities = new BedroomFanEntities(Entities);
        yield return new FanAutomation(fanEntities, Logger);

        var climateEntities = new BedroomClimateEntities(Entities);
        yield return new ClimateAutomation(climateEntities, Scheduler!, Logger);
    }
}
