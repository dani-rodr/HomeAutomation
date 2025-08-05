using System.Linq;

namespace HomeAutomation.apps.Common.Containers;

public class BedroomLightEntities(Devices devices) : IBedroomLightEntities
{
    private readonly Area _area = devices.Bedroom;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public SwitchEntity RightSideEmptySwitch => _area.ExtraControl!.RightSideEmptySwitch!;
    public SwitchEntity LeftSideFanSwitch => _area.FanControl!;
    public ButtonEntity Restart => _area.MotionControl;
}

public class LivingRoomLightEntities(Devices devices) : ILivingRoomLightEntities
{
    private readonly Area _livingRoom = devices.LivingRoom;
    private readonly Area _bedroom = devices.Bedroom;
    private readonly Area _pantry = devices.Pantry;
    private readonly Area _kitchen = devices.Kitchen;
    public SwitchEntity MasterSwitch => _livingRoom.LightControl;
    public BinarySensorEntity MotionSensor => _livingRoom.MotionControl;
    public LightEntity Light => _livingRoom.LightControl;
    public NumberEntity SensorDelay => _livingRoom.MotionControl;
    public SwitchEntity LeftSideFanSwitch => _livingRoom.FanControl!;
    public MediaPlayerEntity TclTv => _livingRoom.MediaControl!;
    public ButtonEntity Restart => _livingRoom.MotionControl;
    public BinarySensorEntity LivingRoomDoor => _livingRoom.ContactSensor!;
    public BinarySensorEntity BedroomDoor => _bedroom.ContactSensor!;
    public BinarySensorEntity BedroomMotionSensor => _bedroom.MotionControl;
    public LightEntity PantryLights => _pantry.LightControl;
    public SwitchEntity PantryMotionAutomation => _pantry.LightControl;
    public BinarySensorEntity PantryMotionSensor => _pantry.MotionControl;
    public BinarySensorEntity KitchenMotionSensor => _kitchen.MotionControl;
}

public class BathroomLightEntities(Devices devices) : IBathroomLightEntities
{
    private readonly Area _area = devices.Bathroom;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public ButtonEntity Restart => _area.MotionControl;
}

public class DeskLightEntities(Devices devices) : IDeskLightEntities
{
    private readonly Area _area = devices.Desk;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public ButtonEntity Restart => _area.MotionControl;
    public LightEntity SalaLights => devices.LivingRoom.LightControl;
}

public class AirQualityEntities(Devices devices) : IAirQualityEntities
{
    private readonly Area _area = devices.LivingRoom;
    private readonly AirPurifierControl _control = devices.LivingRoom.AirPurifierControl!;
    public SwitchEntity MasterSwitch => _control.Automation;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public IEnumerable<SwitchEntity> Fans => [_control.Fan, _area.FanControl!.Fans["StandFan"]];
    public NumericSensorEntity Pm25Sensor => _control.Pm25Sensor;
    public SwitchEntity LedStatus => _control.LedStatus;
    public SwitchEntity LivingRoomFanAutomation => _area.FanControl!.Automation;
}

public class KitchenCookingEntities(Devices devices) : ICookingEntities
{
    private readonly CookingControl _control = devices.Kitchen.CookingControl!;
    public NumericSensorEntity RiceCookerPower => _control.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => _control.RiceCookerSwitch;
    public SensorEntity AirFryerStatus => _control.AirFryerStatus;
    public ButtonEntity InductionTurnOff => _control.InductionTurnOff;
    public NumericSensorEntity InductionPower => _control.InductionPower;
    public SwitchEntity MasterSwitch => _control.Automation;
}

public class BedroomClimateEntities(Devices devices) : IClimateEntities
{
    private readonly Area _area = devices.Bedroom;
    public SwitchEntity MasterSwitch => _area.ClimateControl!;
    public ClimateEntity AirConditioner => _area.ClimateControl!;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public BinarySensorEntity Door => _area.ContactSensor!;
    public SwitchEntity FanAutomation => _area.FanControl!.Automation;
    public BinarySensorEntity HouseMotionSensor => devices.Global.MotionControl;
    public ButtonEntity AcFanModeToggle => _area.ClimateControl!;
    public SwitchEntity Fan => _area.FanControl!;
}

public class ClimateSchedulerEntities(Devices devices) : IClimateSchedulerEntities
{
    private readonly WeatherControl _control = devices.Global.WeatherControl!;
    public SensorEntity SunRising => _control.SunRising;
    public SensorEntity SunSetting => _control.SunSetting;
    public SensorEntity SunMidnight => _control.SunMidnight;
    public WeatherEntity Weather => _control!.Weather;
    public InputBooleanEntity PowerSavingMode => _control.PowerSavingMode;
}

public class DeskDesktopEntities(Devices devices) : IDesktopEntities
{
    private readonly PcControl _control = devices.Desk.PcControl!;
    public SwitchEntity Power => _control.Power;
    public InputButtonEntity RemotePcButton => _control.RemotePcButton;
}

public class BedroomFanEntities(Devices devices) : IBedroomFanEntities
{
    private readonly Area _area = devices.Bedroom;
    public SwitchEntity MasterSwitch => _area.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public IEnumerable<SwitchEntity> Fans => [_area.FanControl!];
}

public class LaptopEntities(Devices devices) : ILaptopEntities
{
    private readonly Area _area = devices.Desk;
    public SwitchEntity VirtualSwitch => _area.LaptopControl!.VirtualSwitch;

    public ButtonEntity WakeOnLanButton => _area.LaptopControl!.WakeOnLanButton;

    public SensorEntity Session => _area.LaptopControl!;
    public NumericSensorEntity BatteryLevel => _area.LaptopControl!;

    public ButtonEntity Lock => _area.LaptopControl!.Lock;
    public ButtonEntity Sleep => _area.LaptopControl!.Sleep;

    public BinarySensorEntity MotionSensor => _area.MotionControl;
}

public class LgDisplayEntities(Devices devices) : ILgDisplayEntities
{
    private readonly Area _area = devices.Desk;
    public MediaPlayerEntity MediaPlayer => _area.MediaControl!;
    public LightEntity Display => _area.LightControl;
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

public class LivingRoomFanEntities(Devices devices) : ILivingRoomFanEntities
{
    private readonly Area _area = devices.LivingRoom;
    public SwitchEntity MasterSwitch => _area.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => _area.SecondaryMotionControl!;
    public IEnumerable<SwitchEntity> Fans => _area.FanControl!.Fans.Values;
    public BinarySensorEntity BedroomMotionSensor => devices.Bedroom.MotionControl;
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

public class PantryLightEntities(Devices devices) : IPantryLightEntities
{
    private readonly Area _area = devices.Pantry;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public BinarySensorEntity MiScalePresenceSensor => _area.MotionLightControl!;
    public LightEntity MirrorLight => _area.MotionLightControl!;
    public ButtonEntity Restart => _area.MotionControl;
    public BinarySensorEntity BedroomDoor => devices.Bedroom.ContactSensor!;
    public SwitchEntity BathroomMotionAutomation => devices.Bathroom.LightControl!.Automation;
    public BinarySensorEntity BathroomMotionSensor => devices.Bathroom.MotionControl!;
}

public class KitchenLightEntities(Devices devices) : IKitchenLightEntities
{
    private readonly Area _area = devices.Kitchen;
    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public BinarySensorEntity PowerPlug => _area.ExtraControl!.PowerPlug!;
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

public class LaptopSchedulerEntities(Devices devices) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => devices.Desk.LaptopControl!.ProjectNationWeek;
}

public class LaptopChargingHandlerEntities(Devices devices) : IChargingHandlerEntities
{
    private readonly LaptopControl _control = devices.Desk.LaptopControl!;
    public NumericSensorEntity Level => _control.Level;
    public SwitchEntity Power => _control.PowerPlug;
}

public class DanielEntities(Devices devices, Entities entities) : IPersonEntities
{
    public PersonEntity Person => devices.Global.PeopleControl!.Daniel.Person;
    public ButtonEntity ToggleLocation => devices.Global.PeopleControl!.Daniel.Toggle;
    public CounterEntity Counter => devices.Global.PeopleControl!.Counter;

    public IEnumerable<BinarySensorEntity> HomeTriggers =>
        [
            entities.BinarySensor.PocoF4GtBle,
            entities.BinarySensor.RedmiWatch5Ble,
            entities.BinarySensor._1921680152,
        ];

    public IEnumerable<BinarySensorEntity> AwayTriggers => [entities.BinarySensor.PocoF4GtBle];

    public IEnumerable<BinarySensorEntity> DirectUnlockTriggers => [];
}

public class AthenaEntities(Devices devices, Entities entities) : IPersonEntities
{
    public PersonEntity Person => devices.Global.PeopleControl!.Athena.Person;
    public ButtonEntity ToggleLocation => devices.Global.PeopleControl!.Athena.Toggle;
    public CounterEntity Counter => devices.Global.PeopleControl!.Counter;

    public IEnumerable<BinarySensorEntity> HomeTriggers =>
        [entities.BinarySensor.MiWatchBle, entities.BinarySensor.Iphone];

    public IEnumerable<BinarySensorEntity> AwayTriggers =>
        [entities.BinarySensor.MiWatchBle, entities.BinarySensor.Iphone];

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
