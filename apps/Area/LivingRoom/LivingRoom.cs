using HomeAutomation.apps.Area.LivingRoom.Automations;

namespace HomeAutomation.apps.Area.LivingRoom;

[NetDaemonApp]
public class LivingRoom
{
    public LivingRoom(Entities entities, ILogger<LivingRoom> logger)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        motionAutomation.StartAutomation();
    }
}
