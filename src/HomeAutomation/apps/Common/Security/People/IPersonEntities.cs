namespace HomeAutomation.apps.Common.Security.People;

public interface IPersonEntities
{
    string Name { get; }
    InputBooleanEntity Presence { get; }
    ButtonEntity ToggleLocation { get; }
    CounterEntity Counter { get; }
    IEnumerable<BinarySensorEntity> HomeTriggers { get; }
    IEnumerable<BinarySensorEntity> AwayTriggers { get; }
    IEnumerable<BinarySensorEntity> DirectUnlockTriggers { get; }
}
