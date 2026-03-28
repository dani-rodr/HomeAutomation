using HomeAutomation.apps.Security;

namespace HomeAutomation.apps.Security.Automations;

public class AccessControlAutomationEntities(SecurityDevices devices) : IAccessControlAutomationEntities
{
    private readonly LockControl _lockControl = devices.LockControl;

    public BinarySensorEntity Door => _lockControl.Door;
    public LockEntity Lock => _lockControl.Lock;
    public BinarySensorEntity House => _lockControl.HouseStatus;
}
