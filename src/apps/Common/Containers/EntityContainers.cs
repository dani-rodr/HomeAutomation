namespace HomeAutomation.apps.Common.Containers;

public class CommonEntities(Entities entities)
{
    // Motion sensors used in multiple classes
    public BinarySensorEntity BedroomMotionSensor { get; } = entities.BinarySensor.BedroomPresenceSensors;
    public BinarySensorEntity LivingRoomMotionSensor { get; } = entities.BinarySensor.LivingRoomPresenceSensors;
    public BinarySensorEntity KitchenMotionSensor { get; } = entities.BinarySensor.KitchenMotionSensors;
    public BinarySensorEntity PantryMotionSensor { get; } = entities.BinarySensor.PantryMotionSensors;
    public BinarySensorEntity HouseMotionSensor { get; } = entities.BinarySensor.House;

    // Shared fan switches
    public SwitchEntity BedroomFanSwitch { get; } = entities.Switch.Sonoff100238104e1;
    public SwitchEntity LivingRoomStandFan { get; } = entities.Switch.Sonoff10023810231;

    // Shared contact sensor
    public BinarySensorEntity BedroomDoor { get; } = entities.BinarySensor.ContactSensorDoor;

    // Pantry lights used in LivingRoom and Pantry
    public LightEntity PantryLights { get; } = entities.Light.PantryLights;

    // Shared master switches
    public SwitchEntity BedroomMotionSwitch { get; } = entities.Switch.BedroomMotionSensor;
    public SwitchEntity LivingRoomMotionSwitch { get; } = entities.Switch.SalaMotionSensor;
    public SwitchEntity PantryMotionSwitch { get; } = entities.Switch.PantryMotionSensor;
}

public class BedroomMotionEntities(Entities entities, CommonEntities common) : IBedroomMotionEntities
{
    public SwitchEntity MasterSwitch => common.BedroomMotionSwitch;
    public BinarySensorEntity MotionSensor => common.BedroomMotionSensor;
    public LightEntity Light => entities.Light.BedLights;
    public NumberEntity SensorDelay => entities.Number.Esp32PresenceBedroomStillTargetDelay;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => common.BedroomFanSwitch;
}

public class LivingRoomMotionEntities(Entities entities, CommonEntities common) : ILivingRoomMotionEntities
{
    public SwitchEntity MasterSwitch => common.LivingRoomMotionSwitch;
    public BinarySensorEntity MotionSensor => common.LivingRoomMotionSensor;
    public LightEntity Light => entities.Light.SalaLightsGroup;
    public NumberEntity SensorDelay => entities.Number.Ld2410Esp321StillTargetDelay;
    public BinarySensorEntity BedroomDoor => common.BedroomDoor;
    public BinarySensorEntity BedroomMotionSensors => common.BedroomMotionSensor;
    public MediaPlayerEntity TclTv => entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity KitchenMotionSensors => common.KitchenMotionSensor;
    public LightEntity PantryLights => common.PantryLights;
    public SwitchEntity PantryMotionSensor => common.PantryMotionSwitch;
    public BinarySensorEntity PantryMotionSensors => common.PantryMotionSensor;
}

public class BathroomMotionEntities(Entities entities) : IBathroomMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BathroomPresenceSensors;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C62StillTargetDelay;
}

public class AirQualityEntities(Entities entities, CommonEntities common) : IAirQualityEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.CleanAir;
    public BinarySensorEntity MotionSensor => common.LivingRoomMotionSensor;
    public SwitchEntity AirPurifierFan => entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch;
    public SwitchEntity SupportingFan => common.LivingRoomStandFan;
    public NumericSensorEntity Pm25Sensor => entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    public SwitchEntity LedStatus => entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;
}

public class KitchenCookingEntities(Entities entities) : ICookingEntities
{
    public NumericSensorEntity RiceCookerPower => entities.Sensor.RiceCookerPower;
    public SwitchEntity RiceCookerSwitch => entities.Switch.RiceCookerSocket1;
    public SensorEntity AirFryerStatus => entities.Sensor.CareliSg593061393Maf05aStatusP21;
    public ButtonEntity InductionTurnOff => entities.Button.InductionCookerPower;
    public NumericSensorEntity InductionPower => entities.Sensor.SmartPlug3SonoffS31Power;
}

public class BedroomClimateEntities(Entities entities, CommonEntities common) : IClimateEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner => entities.Climate.Ac;
    public BinarySensorEntity MotionSensor => common.BedroomMotionSensor;
    public BinarySensorEntity Door => common.BedroomDoor;
    public SwitchEntity FanSwitch => common.BedroomFanSwitch;
    public InputBooleanEntity PowerSavingMode => entities.InputBoolean.AcPowerSavingMode;
    public SensorEntity SunRising => entities.Sensor.SunNextRising;
    public SensorEntity SunSetting => entities.Sensor.SunNextSetting;
    public SensorEntity SunMidnight => entities.Sensor.SunNextMidnight;
    public BinarySensorEntity HouseMotionSensor => common.HouseMotionSensor;
    public ButtonEntity AcFanModeToggle => entities.Button.AcFanModeToggle;
    public WeatherEntity Weather => entities.Weather.Home;
}

public class DeskDesktopEntities(Entities entities) : IDesktopEntities
{
    public BinarySensorEntity PowerPlugThreshold => entities.BinarySensor.SmartPlug1PowerExceedsThreshold;
    public BinarySensorEntity NetworkStatus => entities.BinarySensor.DanielPcNetworkStatus;
    public SwitchEntity PowerSwitch => entities.Switch.WakeOnLan;
    public InputButtonEntity RemotePcButton => entities.InputButton.RemotePc;
}

public class DeskDisplayEntities(Entities entities) : IDisplayEntities
{
    public SwitchEntity LgScreen => entities.Switch.LgScreen;
    public InputNumberEntity LgTvBrightness => entities.InputNumber.LgTvBrightness;
}

public class BedroomFanEntities(CommonEntities common) : IFanEntities
{
    public SwitchEntity MasterSwitch => common.BedroomMotionSwitch;
    public BinarySensorEntity MotionSensor => common.BedroomMotionSensor;
    public IEnumerable<SwitchEntity> Fans => [common.BedroomFanSwitch];
}

public class LaptopEntities(Entities entities) : ILaptopEntities
{
    public SwitchEntity VirtualSwitch => entities.Switch.Laptop;

    public ButtonEntity[] WakeOnLanButtons =>
        [entities.Button.Thinkpadt14WakeOnLan, entities.Button.Thinkpadt14WakeOnWlan];
    public SwitchEntity PowerPlug => entities.Switch.Sonoff1002380fe51;

    public SensorEntity Session => entities.Sensor.Thinkpadt14Sessionstate;

    public NumericSensorEntity BatteryLevel => entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;
    public ButtonEntity Lock => entities.Button.Thinkpadt14Lock;
}

public class DeskLgDisplayEntities(Entities entities) : ILgDisplayEntities
{
    public MediaPlayerEntity LgWebosSmartTv => entities.MediaPlayer.LgWebosSmartTv;
}

public class LivingRoomFanEntities(Entities entities, CommonEntities common) : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => common.LivingRoomMotionSwitch;
    public BinarySensorEntity MotionSensor => common.LivingRoomMotionSensor;
    public IEnumerable<SwitchEntity> Fans =>
        [entities.Switch.CeilingFan, common.LivingRoomStandFan, entities.Switch.Cozylife955f];
    public BinarySensorEntity BedroomMotionSensor => common.BedroomMotionSensor;
}

public class LivingRoomTabletEntities(Entities entities, CommonEntities common) : ITabletEntities
{
    public SwitchEntity MasterSwitch => common.LivingRoomMotionSwitch;
    public BinarySensorEntity MotionSensor => common.LivingRoomMotionSensor;
    public LightEntity TabletScreen => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
}

public class PantryMotionEntities(Entities entities, CommonEntities common) : IPantryMotionEntities
{
    public SwitchEntity MasterSwitch => common.PantryMotionSwitch;
    public BinarySensorEntity MotionSensor => common.PantryMotionSensor;
    public LightEntity Light => common.PantryLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C63StillTargetDelay;
    public BinarySensorEntity MiScalePresenceSensor => entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity BedroomDoor => common.BedroomDoor;
}

public class KitchenMotionEntities(Entities entities, CommonEntities common) : IKitchenMotionEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.KitchenMotionSensor;
    public BinarySensorEntity MotionSensor => common.KitchenMotionSensor;
    public LightEntity Light => entities.Light.RgbLightStrip;
    public NumberEntity SensorDelay => entities.Number.Ld2410Esp325StillTargetDelay;
    public BinarySensorEntity PowerPlug => entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
}

public class LockingEntities(Entities entities, CommonEntities common) : ILockingEntities
{
    public LockEntity Lock => entities.Lock.LockWrapper;

    public BinarySensorEntity Door => entities.BinarySensor.DoorWrapper;

    public BinarySensorEntity HouseStatus => entities.BinarySensor.House;

    public SwitchEntity MasterSwitch => entities.Switch.LockAutomation;

    public BinarySensorEntity MotionSensor => common.HouseMotionSensor;

    public SwitchEntity Flytrap => entities.Switch.Flytrap;
}
