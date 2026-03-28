namespace HomeAutomation.apps.Area.Desk.Devices;

public class LgDisplayEntities(DeskDevices devices) : ILgDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.MediaPlayer;
    public LightEntity Display => devices.Display;
}
