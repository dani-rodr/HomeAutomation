using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

public class BathroomApp(
    IBathroomLightEntities motionEntities,
    ILoggerFactory loggerFactory,
    IScheduler scheduler
) : AppBase<BathroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
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
