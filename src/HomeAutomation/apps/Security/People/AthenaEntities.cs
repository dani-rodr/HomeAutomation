namespace HomeAutomation.apps.Security.People;

public class AthenaEntities(SecurityDevices devices) : IPersonEntities
{
    public PersonEntity Person => devices.AthenaPerson;
    public ButtonEntity ToggleLocation => devices.AthenaToggle;
    public CounterEntity Counter => devices.PeopleCounter;
    public IEnumerable<BinarySensorEntity> HomeTriggers => devices.AthenaHomeTriggers;
    public IEnumerable<BinarySensorEntity> AwayTriggers => devices.AthenaAwayTriggers;
    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers =>
        devices.AthenaDirectUnlockTriggers;
}
