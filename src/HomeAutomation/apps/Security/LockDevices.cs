using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Security;

public class LockDevices(Entities entities, GlobalDevices globalDevices)
{
    public SwitchEntity LockAutomation { get; } = entities.Switch.LockAutomation;
    public LockEntity Lock { get; } = entities.Lock.LockWrapper;
    public BinarySensorEntity Door { get; } = entities.BinarySensor.DoorWrapper;
    public BinarySensorEntity HouseStatus { get; } = entities.BinarySensor.House;
    public SwitchEntity Flytrap { get; } = entities.Switch.Flytrap;
    public BinarySensorEntity GlobalMotionSensor => globalDevices.HouseMotionSensor;
}
