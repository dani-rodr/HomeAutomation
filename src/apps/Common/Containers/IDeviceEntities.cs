namespace HomeAutomation.apps.Common.Containers;

public interface ILgDisplayEntities
{
    MediaPlayerEntity LgWebosSmartTv { get; }
}

public interface IDesktopEntities
{
    BinarySensorEntity PowerPlugThreshold { get; }
    BinarySensorEntity NetworkStatus { get; }
    SwitchEntity PowerSwitch { get; }
}

public class DeskLgDisplayEntities(Entities entities) : ILgDisplayEntities
{
    public MediaPlayerEntity LgWebosSmartTv => entities.MediaPlayer.LgWebosSmartTv;
}

public class DeskDesktopEntities(Entities entities) : IDesktopEntities
{
    public BinarySensorEntity PowerPlugThreshold => entities.BinarySensor.SmartPlug1PowerExceedsThreshold;
    public BinarySensorEntity NetworkStatus => entities.BinarySensor.DanielPcNetworkStatus;
    public SwitchEntity PowerSwitch => entities.Switch.WakeOnLan;
}