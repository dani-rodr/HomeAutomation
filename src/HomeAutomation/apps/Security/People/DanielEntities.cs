using HomeAutomation.apps.Security;
using HomeAutomation.apps.Common.Security.People;

namespace HomeAutomation.apps.Security.People;

public class DanielEntities(SecurityDevices devices) : IPersonEntities
{
    public PersonEntity Person => devices.DanielPerson;
    public ButtonEntity ToggleLocation => devices.DanielToggle;
    public CounterEntity Counter => devices.PeopleCounter;
    public IEnumerable<BinarySensorEntity> HomeTriggers => devices.DanielHomeTriggers;
    public IEnumerable<BinarySensorEntity> AwayTriggers => devices.DanielAwayTriggers;
    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers => [];
}
