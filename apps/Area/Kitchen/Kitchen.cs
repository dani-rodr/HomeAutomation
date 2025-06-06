using HomeAutomation.apps.Area.Kitchen.Automations;

namespace HomeAutomation.apps.Area.Kitchen;

[NetDaemonApp]
public class Kitchen
{
    public Kitchen(Entities entities, ILogger<Kitchen> logger)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        var cookingAutomation = new CookingAutomation(entities, logger);
        motionAutomation.StartAutomation();
        cookingAutomation.StartAutomation();
    }
}
