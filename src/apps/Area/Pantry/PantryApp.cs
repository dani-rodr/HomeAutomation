using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Devices;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(
    IPantryLightEntities motionEntities,
    IScheduler scheduler,
    MotionSensor motionSensor,
    ILoggerFactory loggerFactory
) : AppBase<PantryApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
