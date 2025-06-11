using HomeAutomation.apps.Area.LivingRoom.Automations;

namespace HomeAutomation.apps.Area.LivingRoom;

[NetDaemonApp]
public class LivingRoom
{
    public LivingRoom(Entities entities, ILogger<LivingRoom> logger)
    {
        var lightAutomation = new MotionAutomation(entities, logger);
        lightAutomation.StartAutomation();

        var fanAutomation = new FanAutomation(entities, logger);
        fanAutomation.StartAutomation();
    }
}
