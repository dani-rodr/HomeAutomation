namespace HomeAutomation.apps.Security.People;

public class AthenaEntities(SecurityDevices devices) : IPersonEntities
{
    public string Name => "Athena Bezos";
    public InputBooleanEntity Presence => devices.AthenaPresence;
    public ButtonEntity ToggleLocation => devices.AthenaToggle;
    public CounterEntity Counter => devices.PeopleCounter;
    public IEnumerable<BinarySensorEntity> HomeTriggers => devices.AthenaHomeTriggers;
    public IEnumerable<BinarySensorEntity> AwayTriggers => devices.AthenaAwayTriggers;
    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers =>
        devices.AthenaDirectUnlockTriggers;
}
