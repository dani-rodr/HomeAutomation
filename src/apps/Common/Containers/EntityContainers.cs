namespace HomeAutomation.apps.Common.Containers;

public class CommonEntities(Entities entities)
{
    public MotionSensorGroup MotionSensors => new(entities);
    public FanGroup Fans => new(entities);
    public MotionAutomationGroup MotionAutomations => new(entities);
    public MotionSensorRestartButtonGroup RestartButtons => new(entities);
    public FanAutomationGroup FanAutomations => new(entities);
    public ContactSensorGroup ContactSensors => new(entities);
    public LightGroup Lights => new(entities);
    public DelaySensorGroup DelaySensors => new(entities);

    public class MotionSensorGroup(Entities entities)
    {
        public BinarySensorEntity Bedroom => entities.BinarySensor.BedroomPresenceSensors;
        public BinarySensorEntity Bathroom => entities.BinarySensor.BathroomPresenceSensors;
        public BinarySensorEntity LivingRoom => entities.BinarySensor.LivingRoomPresenceSensors;
        public BinarySensorEntity Kitchen => entities.BinarySensor.KitchenMotionSensors;
        public BinarySensorEntity Pantry => entities.BinarySensor.PantryMotionSensors;
        public BinarySensorEntity Desk => entities.BinarySensor.DeskSmartPresence;
        public BinarySensorEntity House => entities.BinarySensor.House;
    }

    public class MotionSensorRestartButtonGroup(Entities entities)
    {
        public ButtonEntity Bedroom => entities.Button.Esp32PresenceBedroomRestartEsp32;
        public ButtonEntity LivingRoom => entities.Button.Ld2410Esp321RestartEsp32;
        public ButtonEntity Kitchen => entities.Button.Ld2410Esp325RestartEsp32;
        public ButtonEntity Pantry => entities.Button.ZEsp32C63RestartEsp32;
        public ButtonEntity Desk => entities.Button.ZEsp32C61RestartEsp322;
        public ButtonEntity Bathroom => entities.Button.ZEsp32C62RestartEsp32;
    }

    public class FanGroup(Entities entities)
    {
        public SwitchEntity Bedroom => entities.Switch.Sonoff100238104e1;
        public SwitchEntity LivingRoom => entities.Switch.Sonoff10023810231;
    }

    public class FanAutomationGroup(Entities entities)
    {
        public SwitchEntity Bedroom => entities.Switch.BedroomFanAutomation;
        public SwitchEntity LivingRoom => entities.Switch.SalaFanAutomation;
    }

    public class MotionAutomationGroup(Entities entities)
    {
        public SwitchEntity Bedroom => entities.Switch.BedroomMotionSensor;
        public SwitchEntity LivingRoom => entities.Switch.SalaMotionSensor;
        public SwitchEntity Pantry => entities.Switch.PantryMotionSensor;
        public SwitchEntity Tablet => entities.Switch.TabletAutomation;
    }

    public class ContactSensorGroup(Entities entities)
    {
        public BinarySensorEntity Bedroom => entities.BinarySensor.ContactSensorDoor;
    }

    public class LightGroup(Entities entities)
    {
        public LightEntity LivingRoom => entities.Light.SalaLightsGroup;
        public LightEntity Pantry => entities.Light.PantryLights;
    }

    public class DelaySensorGroup(Entities entities)
    {
        public NumberEntity LivingRoom => entities.Number.Ld2410Esp321StillTargetDelay;
    }
}

public class BedroomLightEntities(Entities entities, CommonEntities common) : IBedroomLightEntities
{
    public SwitchEntity MasterSwitch => common.MotionAutomations.Bedroom;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Bedroom;
    public LightEntity Light => entities.Light.BedLights;
    public NumberEntity SensorDelay => entities.Number.Esp32PresenceBedroomStillTargetDelay;
    public SwitchEntity RightSideEmptySwitch => entities.Switch.Sonoff1002352c401;
    public SwitchEntity LeftSideFanSwitch => common.Fans.Bedroom;
    public ButtonEntity Restart => common.RestartButtons.Bedroom;
}

public class LivingRoomLightEntities(Entities entities, CommonEntities common)
    : ILivingRoomLightEntities
{
    public SwitchEntity MasterSwitch => common.MotionAutomations.LivingRoom;
    public BinarySensorEntity MotionSensor => common.MotionSensors.LivingRoom;
    public LightEntity Light => entities.Light.SalaLightsGroup;
    public NumberEntity SensorDelay => common.DelaySensors.LivingRoom;
    public BinarySensorEntity BedroomDoor => common.ContactSensors.Bedroom;
    public BinarySensorEntity BedroomMotionSensors => common.MotionSensors.Bedroom;
    public MediaPlayerEntity TclTv => entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity KitchenMotionSensors => common.MotionSensors.Kitchen;
    public LightEntity PantryLights => common.Lights.Pantry;
    public SwitchEntity PantryMotionSensor => common.MotionAutomations.Pantry;
    public BinarySensorEntity PantryMotionSensors => common.MotionSensors.Pantry;
    public ButtonEntity Restart => common.RestartButtons.LivingRoom;
}

public class BathroomLightEntities(Entities entities, CommonEntities common)
    : IBathroomLightEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomMotionSensor;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Bathroom;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C62StillTargetDelay;
    public ButtonEntity Restart => common.RestartButtons.Bathroom;
}

public class DeskLightEntities(Entities entities, CommonEntities common) : IDeskLightEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.LgTvMotionSensor;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Desk;
    public LightEntity Light => entities.Light.LgDisplay;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C61StillTargetDelay2;
    public LightEntity SalaLights => common.Lights.LivingRoom;
    public ButtonEntity Restart => common.RestartButtons.Desk;
}

public class AirQualityEntities(Entities entities, CommonEntities common) : IAirQualityEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.CleanAir;
    public BinarySensorEntity MotionSensor => common.MotionSensors.LivingRoom;
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
}

public class BedroomClimateEntities(Entities entities, CommonEntities common) : IClimateEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.AcAutomation;
    public ClimateEntity AirConditioner => entities.Climate.Ac;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Bedroom;
    public BinarySensorEntity Door => common.ContactSensors.Bedroom;
    public SwitchEntity FanAutomation => entities.Switch.BedroomFanAutomation;
    public BinarySensorEntity HouseMotionSensor => common.MotionSensors.House;
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

public class BedroomFanEntities(CommonEntities common) : IBedroomFanEntities
{
    public SwitchEntity MasterSwitch => common.FanAutomations.Bedroom;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Bedroom;
    public IEnumerable<SwitchEntity> Fans => [common.Fans.Bedroom];
}

public class LaptopEntities(Entities entities, CommonEntities common) : ILaptopEntities
{
    public SwitchEntity VirtualSwitch => entities.Switch.Laptop;

    public ButtonEntity[] WakeOnLanButtons =>
        [entities.Button.Thinkpadt14WakeOnLan, entities.Button.Thinkpadt14WakeOnWlan];

    public SensorEntity Session => entities.Sensor.Thinkpadt14Sessionstate;

    public NumericSensorEntity BatteryLevel =>
        entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage;
    public ButtonEntity Lock => entities.Button.Thinkpadt14Lock;

    public BinarySensorEntity MotionSensor => common.MotionSensors.Desk;
}

public class LgDisplayEntities(Entities entities) : ILgDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => entities.MediaPlayer.LgWebosSmartTv;
    public LightEntity Display => entities.Light.LgDisplay;
}

public class TclDisplayEntities(Entities entities, CommonEntities common) : ITclDisplayEntities
{
    public MediaPlayerEntity MediaPlayer => entities.MediaPlayer.Tcl65c755;

    public SwitchEntity MasterSwitch => entities.Switch.TvAutomation;

    public BinarySensorEntity MotionSensor => common.MotionSensors.LivingRoom;

    public NumberEntity SensorDelay => common.DelaySensors.LivingRoom;

    public LightEntity Light => entities.Light.TvBacklight3Lite;
    public ButtonEntity Restart => common.RestartButtons.LivingRoom;
}

public class LivingRoomFanEntities(Entities entities, CommonEntities common)
    : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => common.FanAutomations.LivingRoom;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.Ld2410Esp321SmartPresence;
    public IEnumerable<SwitchEntity> Fans =>
        [entities.Switch.CeilingFan, common.Fans.LivingRoom, entities.Switch.Cozylife955f];
    public BinarySensorEntity BedroomMotionSensor => common.MotionSensors.Bedroom;
}

public class LivingRoomTabletEntities(Entities entities, CommonEntities common) : ITabletEntities
{
    public SwitchEntity MasterSwitch => common.MotionAutomations.Tablet;
    public BinarySensorEntity MotionSensor => common.MotionSensors.LivingRoom;
    public LightEntity Light => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
    public NumberEntity SensorDelay => common.DelaySensors.LivingRoom;
    public ButtonEntity Restart => common.RestartButtons.LivingRoom;
}

public class PantryLightEntities(Entities entities, CommonEntities common) : IPantryLightEntities
{
    public SwitchEntity MasterSwitch => common.MotionAutomations.Pantry;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Pantry;
    public LightEntity Light => common.Lights.Pantry;
    public NumberEntity SensorDelay => entities.Number.ZEsp32C63StillTargetDelay;
    public BinarySensorEntity MiScalePresenceSensor =>
        entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
    public LightEntity MirrorLight => entities.Light.ControllerRgbDf1c0d;
    public BinarySensorEntity BedroomDoor => common.ContactSensors.Bedroom;
    public ButtonEntity Restart => common.RestartButtons.Pantry;
}

public class KitchenLightEntities(Entities entities, CommonEntities common) : IKitchenLightEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.KitchenMotionSensor;
    public BinarySensorEntity MotionSensor => common.MotionSensors.Kitchen;
    public LightEntity Light => entities.Light.RgbLightStrip;
    public NumberEntity SensorDelay => entities.Number.Ld2410Esp325StillTargetDelay;
    public BinarySensorEntity PowerPlug => entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
    public ButtonEntity Restart => common.RestartButtons.Kitchen;
}

public class LockingEntities(Entities entities, CommonEntities common) : ILockingEntities
{
    public LockEntity Lock => entities.Lock.LockWrapper;

    public BinarySensorEntity Door => entities.BinarySensor.DoorWrapper;

    public BinarySensorEntity HouseStatus => entities.BinarySensor.House;

    public SwitchEntity MasterSwitch => entities.Switch.LockAutomation;

    public BinarySensorEntity MotionSensor => common.MotionSensors.House;

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
