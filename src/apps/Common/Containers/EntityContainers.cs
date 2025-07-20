using System.Linq;

namespace HomeAutomation.apps.Common.Containers;

public class BedroomLightEntities(Devices devices) : IBedroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.Bedroom.LightControl;
    public BinarySensorEntity MotionSensor => devices.Bedroom.MotionControl;
    public LightEntity Light => devices.Bedroom.LightControl;
    public NumberEntity SensorDelay => devices.Bedroom.MotionControl;
    public SwitchEntity RightSideEmptySwitch => devices.Bedroom.ExtraControl!.RightSideEmptySwitch!;
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
    public MediaPlayerEntity TclTv => devices.LivingRoom.MediaControl!;
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

public class BedroomClimateEntities(Devices devices) : IClimateEntities
{
    public SwitchEntity MasterSwitch => devices.Bedroom.ClimateControl!;
    public ClimateEntity AirConditioner => devices.Bedroom.ClimateControl!;
    public BinarySensorEntity MotionSensor => devices.Bedroom.MotionControl;
    public BinarySensorEntity Door => devices.Bedroom.ContactSensor!;
    public SwitchEntity FanAutomation => devices.Bedroom.FanControl!.Automation;
    public BinarySensorEntity HouseMotionSensor => devices.Global.MotionControl;
    public ButtonEntity AcFanModeToggle => devices.Bedroom.ClimateControl!;
    public SwitchEntity Fan => devices.Bedroom.FanControl!;
}

public class ClimateSchedulerEntities(Devices devices) : IClimateSchedulerEntities
{
    public SensorEntity SunRising => devices.Global.WeatherControl!.SunRising;
    public SensorEntity SunSetting => devices.Global.WeatherControl!.SunSetting;
    public SensorEntity SunMidnight => devices.Global.WeatherControl!.SunMidnight;
    public WeatherEntity Weather => devices.Global.WeatherControl!.Weather;
    public InputBooleanEntity PowerSavingMode => devices.Global.WeatherControl!.PowerSavingMode;
}

public class DeskDesktopEntities(Devices devices) : IDesktopEntities
{
    public SwitchEntity Power => devices.Desk.PcControl!.Power;
    public InputButtonEntity RemotePcButton => devices.Desk.PcControl!.RemotePcButton;
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
    public MediaPlayerEntity MediaPlayer => devices.Desk.MediaControl!;
    public LightEntity Display => devices.Desk.LightControl;
}

public class TclDisplayEntities(Devices devices) : ITclDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => devices.LivingRoom.MediaControl!;

    public SwitchEntity MasterSwitch => devices.LivingRoom.MediaControl!;

    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;

    public NumberEntity SensorDelay => devices.LivingRoom.MotionControl;

    public LightEntity Light => devices.LivingRoom.MediaControl!.Backlight!;

    public ButtonEntity Restart => devices.LivingRoom.MotionControl;
}

public class LivingRoomFanEntities(Devices devices) : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;
    public IEnumerable<SwitchEntity> Fans => devices.LivingRoom.FanControl!.Fans.Values.SkipLast(1);
    public BinarySensorEntity BedroomMotionSensor => devices.Bedroom.MotionControl;
}

public class LivingRoomTabletEntities(Devices devices) : ITabletEntities
{
    public SwitchEntity MasterSwitch => devices.LivingRoom.LightControl;
    public BinarySensorEntity MotionSensor => devices.LivingRoom.MotionControl;
    public LightEntity Light => devices.LivingRoom.MotionLightControl!;
    public BinarySensorEntity TabletActive => devices.LivingRoom.MotionLightControl!;
    public NumberEntity SensorDelay => devices.LivingRoom.MotionControl;
    public ButtonEntity Restart => devices.LivingRoom.MotionControl;
}

public class PantryLightEntities(Devices devices) : IPantryLightEntities
{
    public SwitchEntity MasterSwitch => devices.Pantry.LightControl;
    public BinarySensorEntity MotionSensor => devices.Pantry.MotionControl;
    public LightEntity Light => devices.Pantry.LightControl;
    public NumberEntity SensorDelay => devices.Pantry.MotionControl;
    public BinarySensorEntity MiScalePresenceSensor => devices.Pantry.MotionLightControl!;
    public LightEntity MirrorLight => devices.Pantry.MotionLightControl!;
    public BinarySensorEntity BedroomDoor => devices.Bedroom.ContactSensor!;
    public ButtonEntity Restart => devices.Pantry.MotionControl;
}

public class KitchenLightEntities(Devices devices) : IKitchenLightEntities
{
    public SwitchEntity MasterSwitch => devices.Kitchen.LightControl;
    public BinarySensorEntity MotionSensor => devices.Kitchen.MotionControl;
    public LightEntity Light => devices.Kitchen.LightControl;
    public NumberEntity SensorDelay => devices.Kitchen.MotionControl;
    public BinarySensorEntity PowerPlug => devices.Pantry.ExtraControl!.PowerPlug!;
    public ButtonEntity Restart => devices.Kitchen.MotionControl;
}

public class LockingEntities(Devices devices) : ILockingEntities
{
    public LockEntity Lock => devices.LivingRoom.LockControl!.Lock;

    public BinarySensorEntity Door => devices.LivingRoom.LockControl!.Door;

    public BinarySensorEntity HouseStatus => devices.LivingRoom.LockControl!.HouseStatus;

    public SwitchEntity MasterSwitch => devices.LivingRoom.LockControl!.Automation;

    public BinarySensorEntity MotionSensor => devices.Global.MotionControl;

    public SwitchEntity Flytrap => devices.LivingRoom.LockControl!.Flytrap;
}

public class LaptopSchedulerEntities(Devices devices) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => devices.Desk.LaptopControl!.ProjectNationWeek;
}

public class LaptopChargingHandlerEntities(Devices devices) : IChargingHandlerEntities
{
    public NumericSensorEntity Level => devices.Bedroom.LaptopControl!;
    public SwitchEntity Power => devices.Bedroom.LaptopControl!.PowerPlug;
}
