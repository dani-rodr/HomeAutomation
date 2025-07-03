using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILoggerFactory loggerFactory,
    IBedroomMotionEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IClimateScheduler climateScheduler,
    IScheduler scheduler
) : AppBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            scheduler,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
        yield return new FanAutomation(fanEntities, loggerFactory.CreateLogger<FanAutomation>());
        yield return new ClimateAutomation(
            climateEntities,
            climateScheduler,
            loggerFactory.CreateLogger<ClimateAutomation>()
        );
    }
}
