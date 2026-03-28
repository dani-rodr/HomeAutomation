using HomeAutomation.apps.Security;

namespace HomeAutomation.apps.Common.Security.Automations;

public class LockingEntities(LockDevices devices) : ILockingEntities
{
    public LockEntity Lock => devices.Lock;
    public BinarySensorEntity Door => devices.Door;
    public BinarySensorEntity HouseStatus => devices.HouseStatus;
    public SwitchEntity MasterSwitch => devices.LockAutomation;
    public BinarySensorEntity MotionSensor => devices.GlobalMotionSensor;
    public SwitchEntity Flytrap => devices.Flytrap;
}
