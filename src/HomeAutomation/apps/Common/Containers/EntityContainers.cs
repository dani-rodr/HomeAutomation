namespace HomeAutomation.apps.Common.Containers;

public class ClimateSchedulerEntities(Devices devices) : IClimateSchedulerEntities
{
    private readonly WeatherControl _control = devices.Global.WeatherControl!;
    public SensorEntity SunRising => _control.SunRising;
    public SensorEntity SunSetting => _control.SunSetting;
    public SensorEntity SunMidnight => _control.SunMidnight;
    public WeatherEntity Weather => _control!.Weather;
    public InputBooleanEntity PowerSavingMode => _control.PowerSavingMode;
}

public class TclDisplayEntities(Devices devices) : ITclDisplayEntities
{
    private readonly Area _area = devices.LivingRoom;
    public MediaPlayerEntity MediaPlayer => _area.MediaControl!;
    public SwitchEntity MasterSwitch => _area.MediaControl!;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public LightEntity Light => _area.MediaControl!.Backlight!;
    public ButtonEntity Restart => _area.MotionControl;
}

public class LivingRoomTabletEntities(Devices devices) : ITabletEntities
{
    private readonly Area _area = devices.LivingRoom;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.MotionLightControl!;
    public BinarySensorEntity TabletActive => _area.MotionLightControl!;
    public NumberEntity SensorDelay => _area.MotionControl;
    public ButtonEntity Restart => _area.MotionControl;
}

public class LockingEntities(Devices devices) : ILockingEntities
{
    private readonly LockControl _control = devices.LivingRoom.LockControl!;
    public LockEntity Lock => _control.Lock;
    public BinarySensorEntity Door => _control.Door;
    public BinarySensorEntity HouseStatus => _control.HouseStatus;
    public SwitchEntity MasterSwitch => _control.Automation;
    public BinarySensorEntity MotionSensor => devices.Global.MotionControl;
    public SwitchEntity Flytrap => _control.Flytrap;
}

public class DanielEntities(Devices devices, Entities entities) : IPersonEntities
{
    public PersonEntity Person => devices.Global.PeopleControl!.Daniel.Person;
    public ButtonEntity ToggleLocation => devices.Global.PeopleControl!.Daniel.Toggle;
    public CounterEntity Counter => devices.Global.PeopleControl!.Counter;

    public IEnumerable<BinarySensorEntity> HomeTriggers =>
        [entities.BinarySensor.RedmiWatch5Ble, entities.BinarySensor.Oneplus13Ble];

    public IEnumerable<BinarySensorEntity> AwayTriggers =>
        [entities.BinarySensor.PocoF4GtBle, entities.BinarySensor.Oneplus13Ble];

    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers => [];
}

public class AthenaEntities(Devices devices, Entities entities) : IPersonEntities
{
    public PersonEntity Person => devices.Global.PeopleControl!.Athena.Person;
    public ButtonEntity ToggleLocation => devices.Global.PeopleControl!.Athena.Toggle;
    public CounterEntity Counter => devices.Global.PeopleControl!.Counter;

    public IEnumerable<BinarySensorEntity> HomeTriggers =>
        [entities.BinarySensor.MiWatchBle, entities.BinarySensor.Iphone];

    public IEnumerable<BinarySensorEntity> AwayTriggers => [entities.BinarySensor.Iphone];

    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers =>
        [entities.BinarySensor.BaseusTagBle];
}

public class AccessControlAutomationEntities(Devices devices) : IAccessControlAutomationEntities
{
    private readonly LockControl _lockControl = devices.LivingRoom.LockControl!;
    public BinarySensorEntity Door => _lockControl.Door;
    public LockEntity Lock => _lockControl.Lock;
    public BinarySensorEntity House => _lockControl.HouseStatus;
}
