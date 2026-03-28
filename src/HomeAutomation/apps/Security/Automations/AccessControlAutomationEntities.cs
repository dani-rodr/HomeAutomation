namespace HomeAutomation.apps.Security.Automations;

public class AccessControlAutomationEntities(SecurityDevices devices)
    : IAccessControlAutomationEntities
{
    public BinarySensorEntity Door => devices.Door;
    public LockEntity Lock => devices.Lock;
    public BinarySensorEntity House => devices.HouseStatus;
}
