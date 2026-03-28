using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Security;

public class SecurityDevices(GlobalDevices globalDevices, Entities entities)
{
    public PersonEntity DanielPerson => globalDevices.DanielPerson;
    public ButtonEntity DanielToggle => globalDevices.DanielToggle;

    public PersonEntity AthenaPerson => globalDevices.AthenaPerson;
    public ButtonEntity AthenaToggle => globalDevices.AthenaToggle;

    public CounterEntity PeopleCounter => globalDevices.PeopleCounter;

    public IEnumerable<BinarySensorEntity> DanielHomeTriggers =>
        [entities.BinarySensor.RedmiWatch5Ble, entities.BinarySensor.Oneplus13Ble];

    public IEnumerable<BinarySensorEntity> DanielAwayTriggers =>
        [entities.BinarySensor.PocoF4GtBle, entities.BinarySensor.Oneplus13Ble];

    public IEnumerable<BinarySensorEntity> AthenaHomeTriggers =>
        [entities.BinarySensor.MiWatchBle, entities.BinarySensor.Iphone];

    public IEnumerable<BinarySensorEntity> AthenaAwayTriggers => [entities.BinarySensor.Iphone];

    public IEnumerable<BinarySensorEntity> AthenaDirectUnlockTriggers =>
        [entities.BinarySensor.BaseusTagBle];
}
