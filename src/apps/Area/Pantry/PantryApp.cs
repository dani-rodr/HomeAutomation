using HomeAutomation.apps.Area.Pantry.Automations;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(IPantryMotionEntities motionEntities, ILogger<PantryApp> logger) : AreaBase<PantryApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(motionEntities, logger);
    }
}
