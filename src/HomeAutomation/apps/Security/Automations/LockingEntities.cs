using HomeAutomation.apps.Security;

namespace HomeAutomation.apps.Common.Security.Automations;

public class LockingEntities(SecurityDevices devices) : ILockingEntities
{
    private readonly LockControl _control = devices.LockControl;

    public LockEntity Lock => _control.Lock;
    public BinarySensorEntity Door => _control.Door;
    public BinarySensorEntity HouseStatus => _control.HouseStatus;
    public SwitchEntity MasterSwitch => _control.Automation;
    public BinarySensorEntity MotionSensor => devices.GlobalMotionSensor;
    public SwitchEntity Flytrap => _control.Flytrap;
}
