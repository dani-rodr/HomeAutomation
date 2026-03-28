namespace HomeAutomation.apps.Area.Desk.Devices.Entities;

public class DeskDesktopEntities(DeskDevices devices) : IDesktopEntities
{
    public SwitchEntity Power => devices.DesktopPower;
    public InputButtonEntity RemotePcButton => devices.RemotePcButton;
}
