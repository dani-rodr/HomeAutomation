using HomeAutomation.apps.Area.Pantry.Automations;

namespace HomeAutomation.apps.Area.Pantry;

[NetDaemonApp]
public class Pantry
{
    public Pantry(Entities entities, ILogger<Pantry> logger)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        motionAutomation.StartAutomation();
    }
}
