namespace HomeAutomation.apps.Area.Desk.Devices.Entities;

public class LgDisplayEntities(DeskDevices devices) : ILgDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.MediaPlayer;
    public LightEntity Display => devices.Display;
}
