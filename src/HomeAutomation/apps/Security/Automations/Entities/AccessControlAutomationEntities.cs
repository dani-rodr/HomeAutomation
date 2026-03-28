namespace HomeAutomation.apps.Security.Automations.Entities;

public class AccessControlAutomationEntities(LockDevices devices) : IAccessControlAutomationEntities
{
    public BinarySensorEntity Door => devices.Door;
    public LockEntity Lock => devices.Lock;
    public BinarySensorEntity House => devices.HouseStatus;
}
