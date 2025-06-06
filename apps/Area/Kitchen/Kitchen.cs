using HomeAutomation.apps.Area.Kitchen.Automations;

namespace HomeAutomation.apps.Area.Kitchen;

// The Kitchen class uses composition to instantiate and manage MotionAutomation and CookingAutomation
// instead of inheriting from a base class like MotionAutomationBase. This design choice improves modularity
// and allows for more flexible reuse of automation components.

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
