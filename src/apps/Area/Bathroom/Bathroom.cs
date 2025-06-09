using HomeAutomation.apps.Area.Bathroom.Automations;

namespace HomeAutomation.apps.Area.Bathroom;

[NetDaemonApp]
public class Bathroom
{
    public Bathroom(Entities entities, ILogger<Bathroom> logger)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        motionAutomation.StartAutomation();
    }
}
