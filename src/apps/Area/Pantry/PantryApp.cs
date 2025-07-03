using HomeAutomation.apps.Area.Pantry.Automations;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(IPantryLightEntities motionEntities, ILoggerFactory loggerFactory)
    : AppBase<PantryApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new LightAutomation(
            motionEntities,
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
