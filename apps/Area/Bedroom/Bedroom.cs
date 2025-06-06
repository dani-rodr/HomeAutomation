using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

[NetDaemonApp]
public class Bedroom
{
    public Bedroom(Entities entities, ILogger<Bedroom> logger)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        motionAutomation.StartAutomation();
    }
}
