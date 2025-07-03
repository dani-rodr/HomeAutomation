using HomeAutomation.apps.Area.Pantry.Automations;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(IPantryMotionEntities motionEntities, ILoggerFactory loggerFactory)
    : AppBase<PantryApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(
            motionEntities,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
    }
}
