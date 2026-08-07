namespace HomeAutomation.apps.Security.People;

public class DanielEntities(SecurityDevices devices) : IPersonEntities
{
    public string Name => "Daniel Rodriguez";
    public InputBooleanEntity Presence => devices.DanielPresence;
    public ButtonEntity ToggleLocation => devices.DanielToggle;
    public CounterEntity Counter => devices.PeopleCounter;
    public IEnumerable<BinarySensorEntity> HomeTriggers => devices.DanielHomeTriggers;
    public IEnumerable<BinarySensorEntity> AwayTriggers => devices.DanielAwayTriggers;
    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers => [];
}
