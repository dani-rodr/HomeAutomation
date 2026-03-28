namespace HomeAutomation.apps.Common.Security.Automations;

public interface ILockingEntities : IMotionBase
{
    LockEntity Lock { get; }
    BinarySensorEntity Door { get; }
    BinarySensorEntity HouseStatus { get; }
    SwitchEntity Flytrap { get; }
}
