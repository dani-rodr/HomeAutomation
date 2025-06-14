namespace HomeAutomation.apps.Common.Containers;

public interface IDesktopEntities
{
    BinarySensorEntity PowerPlugThreshold { get; }
    BinarySensorEntity NetworkStatus { get; }
    SwitchEntity PowerSwitch { get; }
    InputButtonEntity RemotePcButton { get; }
}

public class DeskDesktopEntities(Entities entities) : IDesktopEntities
{
    public BinarySensorEntity PowerPlugThreshold => entities.BinarySensor.SmartPlug1PowerExceedsThreshold;
    public BinarySensorEntity NetworkStatus => entities.BinarySensor.DanielPcNetworkStatus;
    public SwitchEntity PowerSwitch => entities.Switch.WakeOnLan;
    public InputButtonEntity RemotePcButton => entities.InputButton.RemotePc;
}
