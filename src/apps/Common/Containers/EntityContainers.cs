namespace HomeAutomation.apps.Common.Containers;

public class CommonEntities(Entities entities)
{
    public Fans Fans => new(entities);
    public FanAutomations FanAutomations => new(entities);
    public ContactSensors ContactSensors => new(entities);
    public Lights Lights => new(entities);
    public BinarySensorEntity House => entities.BinarySensor.House;
}

public class Fans(Entities entities)
{
    public SwitchEntity Bedroom => entities.Switch.Sonoff100238104e1;
    public SwitchEntity LivingRoom => entities.Switch.Sonoff10023810231;
}

public class FanAutomations(Entities entities)
{
    public SwitchEntity Bedroom => entities.Switch.BedroomFanAutomation;
    public SwitchEntity LivingRoom => entities.Switch.SalaFanAutomation;
}

public class ContactSensors(Entities entities)
{
    public BinarySensorEntity Bedroom => entities.BinarySensor.ContactSensorDoor;
    public BinarySensorEntity LivingRoom => entities.BinarySensor.DoorWrapper;
}

public class Lights(Entities entities)
{
    public LightEntity LivingRoom => entities.Light.SalaLightsGroup;
    public LightEntity Pantry => entities.Light.PantryLights;
}

public class BedroomLightEntities(Entities entities, Devices devices, CommonEntities common)
    : IBedroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.MotionSensors.Bedroom.Automation;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Bedroom.Sensor;
    public LightEntity Light => entities.Light.BedLights;
    public NumberEntity SensorDelay => devices.MotionSensors.Bedroom.Timer;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => common.Fans.Bedroom;
    public ButtonEntity Restart => devices.MotionSensors.Bedroom.Restart;
}

public class LivingRoomLightEntities(Entities entities, Devices devices, CommonEntities common)
    : ILivingRoomLightEntities
{
    public SwitchEntity MasterSwitch => devices.MotionSensors.LivingRoom.Automation;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.LivingRoom.Sensor;
    public LightEntity Light => entities.Light.SalaLightsGroup;
    public NumberEntity SensorDelay => devices.MotionSensors.LivingRoom.Timer;
    public SwitchEntity LeftSideFanSwitch => common.Fans.LivingRoom;
    public BinarySensorEntity BedroomDoor => common.ContactSensors.Bedroom;
    public BinarySensorEntity BedroomMotionSensor => devices.MotionSensors.Bedroom.Sensor;
    public MediaPlayerEntity TclTv => entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity KitchenMotionSensor => devices.MotionSensors.Kitchen.Sensor;
    public LightEntity PantryLights => common.Lights.Pantry;
    public SwitchEntity PantryMotionAutomation => devices.MotionSensors.Pantry.Automation;
    public BinarySensorEntity PantryMotionSensor => devices.MotionSensors.Pantry.Sensor;
    public ButtonEntity Restart => devices.MotionSensors.LivingRoom.Restart;
    public BinarySensorEntity LivingRoomDoor => common.ContactSensors.LivingRoom;
}

public class BathroomLightEntities(Entities entities, Devices devices) : IBathroomLightEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Bathroom.Sensor;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => devices.MotionSensors.Bathroom.Timer;
    public ButtonEntity Restart => devices.MotionSensors.Bathroom.Restart;
}

public class DeskLightEntities(Entities entities, Devices devices, CommonEntities common)
    : IDeskLightEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.LgTvMotionSensor;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Desk.Sensor;
    public LightEntity Light => entities.Light.LgDisplay;
    public NumberEntity SensorDelay => devices.MotionSensors.Desk.Timer;
    public LightEntity SalaLights => common.Lights.LivingRoom;
    public ButtonEntity Restart => devices.MotionSensors.Desk.Restart;
}

public class AirQualityEntities(Entities entities, Devices devices, CommonEntities common)
    : IAirQualityEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.CleanAir;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.LivingRoom.Sensor;

    public IEnumerable<SwitchEntity> Fans =>
        [
            entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch,
            common.Fans.LivingRoom,
        ];
    public NumericSensorEntity Pm25Sensor => entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    public SwitchEntity LedStatus =>
        entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;

    public SwitchEntity LivingRoomFanAutomation => common.FanAutomations.LivingRoom;
}

public class KitchenCookingEntities(Entities entities) : ICookingEntities
{
    public NumericSensorEntity RiceCookerPower => entities.Sensor.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => entities.Switch.RiceCookerSocket1;
    public SensorEntity AirFryerStatus => entities.Sensor.CareliSg593061393Maf05aStatusP21;
    public ButtonEntity InductionTurnOff => entities.Button.InductionCookerPower;
    public NumericSensorEntity InductionPower => entities.Sensor.SmartPlug3SonoffS31Power;
    public SwitchEntity MasterSwitch => entities.Switch.CookingAutomation;
}

public class BedroomClimateEntities(Entities entities, Devices devices, CommonEntities common)
    : IClimateEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner => entities.Climate.Ac;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Bedroom.Sensor;
    public BinarySensorEntity Door => common.ContactSensors.Bedroom;
    public SwitchEntity FanAutomation => entities.Switch.BedroomFanAutomation;
    public BinarySensorEntity HouseMotionSensor => common.House;
    public ButtonEntity AcFanModeToggle => entities.Button.AcFanModeToggle;
    public SwitchEntity Fan => common.Fans.Bedroom;
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

public class BedroomFanEntities(CommonEntities common, Devices devices) : IBedroomFanEntities
{
    public SwitchEntity MasterSwitch => common.FanAutomations.Bedroom;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Bedroom.Sensor;
    public IEnumerable<SwitchEntity> Fans => [common.Fans.Bedroom];
}

public class LaptopEntities(Entities entities, Devices devices) : ILaptopEntities
{
    public SwitchEntity VirtualSwitch => entities.Switch.Laptop;

    public ButtonEntity[] WakeOnLanButtons =>
        [entities.Button.Thinkpadt14WakeOnLan, entities.Button.Thinkpadt14WakeOnWlan];

    public SensorEntity Session => entities.Sensor.Thinkpadt14Sessionstate;

    public NumericSensorEntity BatteryLevel =>
        entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;
    public ButtonEntity Lock => entities.Button.Thinkpadt14Lock;

    public BinarySensorEntity MotionSensor => devices.MotionSensors.Desk.Sensor;
}

public class LgDisplayEntities(Entities entities) : ILgDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => entities.MediaPlayer.LgWebosSmartTv;
    public LightEntity Display => entities.Light.LgDisplay;
}

public class TclDisplayEntities(Entities entities, Devices devices) : ITclDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => entities.MediaPlayer.Tcl65c755;

    public SwitchEntity MasterSwitch => entities.Switch.TvAutomation;

    public BinarySensorEntity MotionSensor => devices.MotionSensors.LivingRoom.Sensor;

    public NumberEntity SensorDelay => devices.MotionSensors.LivingRoom.Timer;

    public LightEntity Light => entities.Light.TvBacklight3Lite;
    public ButtonEntity Restart => devices.MotionSensors.LivingRoom.Restart;
}

public class LivingRoomFanEntities(Entities entities, Devices devices, CommonEntities common)
    : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => common.FanAutomations.LivingRoom;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.LivingRoom.Sensor;
    public IEnumerable<SwitchEntity> Fans =>
        [entities.Switch.CeilingFan, common.Fans.LivingRoom, entities.Switch.Cozylife955f];
    public BinarySensorEntity BedroomMotionSensor => devices.MotionSensors.Bedroom.Sensor;
}

public class LivingRoomTabletEntities(Entities entities, Devices devices) : ITabletEntities
{
    public SwitchEntity MasterSwitch => devices.MotionSensors.LivingRoom.Automation;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.LivingRoom.Sensor;
    public LightEntity Light => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
    public NumberEntity SensorDelay => devices.MotionSensors.LivingRoom.Timer;
    public ButtonEntity Restart => devices.MotionSensors.LivingRoom.Restart;
}

public class PantryLightEntities(Entities entities, Devices devices, CommonEntities common)
    : IPantryLightEntities
{
    public SwitchEntity MasterSwitch => devices.MotionSensors.Pantry.Automation;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Pantry.Sensor;
    public LightEntity Light => common.Lights.Pantry;
    public NumberEntity SensorDelay => devices.MotionSensors.Pantry.Timer;
    public BinarySensorEntity MiScalePresenceSensor =>
        entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity BedroomDoor => common.ContactSensors.Bedroom;
    public ButtonEntity Restart => devices.MotionSensors.Pantry.Restart;
}

public class KitchenLightEntities(Entities entities, Devices devices) : IKitchenLightEntities
{
    public SwitchEntity MasterSwitch => devices.MotionSensors.Kitchen.Automation;
    public BinarySensorEntity MotionSensor => devices.MotionSensors.Kitchen.Sensor;
    public LightEntity Light => entities.Light.RgbLightStrip;
    public NumberEntity SensorDelay => devices.MotionSensors.Kitchen.Timer;
    public BinarySensorEntity PowerPlug => entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
    public ButtonEntity Restart => devices.MotionSensors.Kitchen.Restart;
}

public class LockingEntities(Entities entities, CommonEntities common) : ILockingEntities
{
    public LockEntity Lock => entities.Lock.LockWrapper;

    public BinarySensorEntity Door => entities.BinarySensor.DoorWrapper;

    public BinarySensorEntity HouseStatus => entities.BinarySensor.House;

    public SwitchEntity MasterSwitch => entities.Switch.LockAutomation;

    public BinarySensorEntity MotionSensor => common.House;

    public SwitchEntity Flytrap => entities.Switch.Flytrap;
}

public class LaptopSchedulerEntities(Entities entities) : ILaptopSchedulerEntities
{
    public InputBooleanEntity ProjectNationWeek => entities.InputBoolean.ProjectNationWeek;
}

public class LaptopChargingHandlerEntities(Entities entities) : IChargingHandlerEntities
{
    public NumericSensorEntity Level => entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;

    public SwitchEntity Power => entities.Switch.Sonoff1002380fe51;
}
