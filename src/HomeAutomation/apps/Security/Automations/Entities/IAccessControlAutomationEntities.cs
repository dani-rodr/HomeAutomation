namespace HomeAutomation.apps.Security.Automations.Entities;

public interface IAccessControlAutomationEntities
{
    BinarySensorEntity Door { get; }
    BinarySensorEntity House { get; }
    LockEntity Lock { get; }
}
