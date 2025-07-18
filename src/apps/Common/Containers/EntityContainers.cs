using System.Linq;

namespace HomeAutomation.apps.Common.Containers;

public class BedroomLightEntities(Entities entities, Devices devices) : IBedroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.Bedroom.LightControl;
    public BinarySensorEntity MotionSensor => devices.Bedroom.MotionControl;
    public LightEntity Light => devices.Bedroom.LightControl;
    public NumberEntity SensorDelay => devices.Bedroom.MotionControl;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => devices.Bedroom.FanControl!;
    public ButtonEntity Restart => devices.Bedroom.MotionControl;
}

public class LivingRoomLightEntities(Devices devices) : ILivingRoomLightEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.LightControl;
    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;
    public LightEntity Light => devices.LivingRoom.LightControl;
    public NumberEntity SensorDelay => devices.LivingRoom.MotionControl;
    public SwitchEntity LeftSideFanSwitch => devices.LivingRoom.FanControl!;
    public BinarySensorEntity BedroomDoor => devices.Bedroom.ContactSensor!;
    public BinarySensorEntity BedroomMotionSensor => devices.Bedroom.MotionControl;
    public MediaPlayerEntity TclTv => devices.LivingRoom.MediaPlayer!;
    public BinarySensorEntity KitchenMotionSensor => devices.Kitchen.MotionControl;
    public LightEntity PantryLights => devices.Pantry.LightControl;
    public SwitchEntity PantryMotionAutomation => devices.Pantry.LightControl;
    public BinarySensorEntity PantryMotionSensor => devices.Pantry.MotionControl;
    public ButtonEntity Restart => devices.LivingRoom.MotionControl;
    public BinarySensorEntity LivingRoomDoor => devices.LivingRoom.ContactSensor!;
}

public class BathroomLightEntities(Devices devices) : IBathroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.Bathroom.LightControl;
    public BinarySensorEntity MotionSensor => devices.Bathroom.MotionControl;
    public LightEntity Light => devices.Bathroom.LightControl;
    public NumberEntity SensorDelay => devices.Bathroom.MotionControl;
    public ButtonEntity Restart => devices.Bathroom.MotionControl;
}

public class DeskLightEntities(Devices devices) : IDeskLightEntities
{
    public SwitchEntity MasterSwitch => devices.Desk.LightControl;
    public BinarySensorEntity MotionSensor => devices.Desk.MotionControl;
    public LightEntity Light => devices.Desk.LightControl;
    public NumberEntity SensorDelay => devices.Desk.MotionControl;
    public LightEntity SalaLights => devices.LivingRoom.LightControl;
    public ButtonEntity Restart => devices.Desk.MotionControl;
}

public class AirQualityEntities(Devices devices) : IAirQualityEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.AirPurifierControl!.Automation;

    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;

    public IEnumerable<SwitchEntity> Fans =>
        [devices.LivingRoom.AirPurifierControl!.Fan, devices.LivingRoom.FanControl!];
    public NumericSensorEntity Pm25Sensor => devices.LivingRoom.AirPurifierControl!.Pm25Sensor;
    public SwitchEntity LedStatus => devices.LivingRoom.AirPurifierControl!.LedStatus;

    public SwitchEntity LivingRoomFanAutomation => devices.LivingRoom.FanControl!.Automation;
}

public class KitchenCookingEntities(Devices devices) : ICookingEntities
{
    public NumericSensorEntity RiceCookerPower => devices.Kitchen.CookingControl!.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => devices.Kitchen.CookingControl!.RiceCookerSwitch;
    public SensorEntity AirFryerStatus => devices.Kitchen.CookingControl!.AirFryerStatus;
    public ButtonEntity InductionTurnOff => devices.Kitchen.CookingControl!.InductionTurnOff;
    public NumericSensorEntity InductionPower => devices.Kitchen.CookingControl!.InductionPower;
    public SwitchEntity MasterSwitch => devices.Kitchen.CookingControl!.Automation;
}

public class BedroomClimateEntities(Entities entities, Devices devices) : IClimateEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner => entities.Climate.Ac;
    public BinarySensorEntity MotionSensor => devices.Bedroom.MotionControl;
    public BinarySensorEntity Door => devices.Bedroom.ContactSensor!;
    public SwitchEntity FanAutomation => devices.Bedroom.FanControl!.Automation;
    public BinarySensorEntity HouseMotionSensor => devices.GlobalEntities.HouseOccupancy;
    public ButtonEntity AcFanModeToggle => entities.Button.AcFanModeToggle;
    public SwitchEntity Fan => devices.Bedroom.FanControl!;
}

public class ClimateSchedulerEntities(Entities entities) : IClimateSchedulerEntities
{
    public SensorEntity SunRising => entities.Sensor.SunNextRising;
    public SensorEntity SunSetting => entities.Sensor.SunNextSetting;
    public SensorEntity SunMidnight => entities.Sensor.SunNextMidnight;
    public WeatherEntity Weather => entities.Weather.Home;
    public InputBooleanEntity PowerSavingMode => entities.InputBoolean.AcPowerSavingMode;
}

public class DeskDesktopEntities(Entities entities) : IDesktopEntities
{
    public SwitchEntity Power => entities.Switch.DanielPc;
    public InputButtonEntity RemotePcButton => entities.InputButton.RemotePc;
}

public class BedroomFanEntities(Devices devices) : IBedroomFanEntities
{
    public SwitchEntity MasterSwitch => devices.Bedroom.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => devices.Bedroom.MotionControl;
    public IEnumerable<SwitchEntity> Fans => [devices.Bedroom.FanControl!];
}

public class LaptopEntities(Devices devices) : ILaptopEntities
{
    public SwitchEntity VirtualSwitch => devices.Bedroom.LaptopControl!.VirtualSwitch;

    public ButtonEntity WakeOnLanButton => devices.Bedroom.LaptopControl!.WakeOnLanButton;

    public SensorEntity Session => devices.Bedroom.LaptopControl!;
    public NumericSensorEntity BatteryLevel => devices.Bedroom.LaptopControl!;

    public ButtonEntity Lock => devices.Bedroom.LaptopControl!.Lock;

    public BinarySensorEntity MotionSensor => devices.Desk.MotionControl;
}

public class LgDisplayEntities(Devices devices) : ILgDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.Desk.MediaPlayer!;
    public LightEntity Display => devices.Desk.LightControl;
}

public class TclDisplayEntities(Devices devices) : ITclDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.LivingRoom.MediaPlayer!;

    public SwitchEntity MasterSwitch => devices.LivingRoom.MediaPlayer!;

    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;

    public NumberEntity SensorDelay => devices.LivingRoom.MotionControl;

    public LightEntity Light => devices.LivingRoom.MediaPlayer!.Backlight!;

    public ButtonEntity Restart => devices.LivingRoom.MotionControl;
}

public class LivingRoomFanEntities(Devices devices) : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;
    public IEnumerable<SwitchEntity> Fans => devices.LivingRoom.FanControl!.Fans.Values.SkipLast(1);
    public BinarySensorEntity BedroomMotionSensor => devices.Bedroom.MotionControl;
}

public class LivingRoomTabletEntities(Entities entities, Devices devices) : ITabletEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.LightControl;
    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;
    public LightEntity Light => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
    public NumberEntity SensorDelay => devices.LivingRoom.MotionControl;
    public ButtonEntity Restart => devices.LivingRoom.MotionControl;
}

public class PantryLightEntities(Entities entities, Devices devices) : IPantryLightEntities
{
    public SwitchEntity MasterSwitch => devices.Pantry.LightControl;
    public BinarySensorEntity MotionSensor => devices.Pantry.MotionControl;
    public LightEntity Light => devices.Pantry.LightControl;
    public NumberEntity SensorDelay => devices.Pantry.MotionControl;
    public BinarySensorEntity MiScalePresenceSensor =>
        entities.BinarySensor.BedroomMotionSensorMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity BedroomDoor => devices.Bedroom.ContactSensor!;
    public ButtonEntity Restart => devices.Pantry.MotionControl;
}

public class KitchenLightEntities(Entities entities, Devices devices) : IKitchenLightEntities
{
    public SwitchEntity MasterSwitch => devices.Kitchen.LightControl;
    public BinarySensorEntity MotionSensor => devices.Kitchen.MotionControl;
    public LightEntity Light => devices.Kitchen.LightControl;
    public NumberEntity SensorDelay => devices.Kitchen.MotionControl;
    public BinarySensorEntity PowerPlug => entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
    public ButtonEntity Restart => devices.Kitchen.MotionControl;
}

public class LockingEntities(Entities entities, Devices devices) : ILockingEntities
{
    public LockEntity Lock => entities.Lock.LockWrapper;

    public BinarySensorEntity Door => entities.BinarySensor.DoorWrapper;

    public BinarySensorEntity HouseStatus => entities.BinarySensor.House;

    public SwitchEntity MasterSwitch => entities.Switch.LockAutomation;

    public BinarySensorEntity MotionSensor => devices.GlobalEntities.HouseOccupancy;

    public SwitchEntity Flytrap => entities.Switch.Flytrap;
}

public class LaptopSchedulerEntities(Entities entities) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => entities.InputBoolean.ProjectNationWeek;
}

public class LaptopChargingHandlerEntities(Devices devices) : IChargingHandlerEntities
{
    public NumericSensorEntity Level => devices.Bedroom.LaptopControl!;
    public SwitchEntity Power => devices.Bedroom.LaptopControl!.PowerPlug;
}
