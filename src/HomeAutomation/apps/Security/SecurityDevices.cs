namespace HomeAutomation.apps.Security;

public class SecurityDevices(Devices devices, Entities entities)
{
    public LockControl LockControl => devices.LivingRoom.LockControl!;
    public BinarySensorEntity GlobalMotionSensor => devices.Global.MotionControl;

    public PersonEntity DanielPerson => devices.Global.PeopleControl!.Daniel.Person;
    public ButtonEntity DanielToggle => devices.Global.PeopleControl!.Daniel.Toggle;

    public PersonEntity AthenaPerson => devices.Global.PeopleControl!.Athena.Person;
    public ButtonEntity AthenaToggle => devices.Global.PeopleControl!.Athena.Toggle;

    public CounterEntity PeopleCounter => devices.Global.PeopleControl!.Counter;

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
