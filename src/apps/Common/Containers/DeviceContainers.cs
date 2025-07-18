using System.Linq;

namespace HomeAutomation.apps.Common.Containers;

public class Devices(Entities entities)
{
    public GlobalEntities GlobalEntities { get; } = new(entities);
    public Area Bedroom { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.BedroomPresenceSensors,
                entities.Button.BedroomMotionSensorRestartEsp32,
                entities.Number.BedroomMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.BedroomMotionSensor, entities.Light.BedLights),
            new FanControl(
                entities.Switch.BedroomFanAutomation,
                new Dictionary<string, SwitchEntity>
                {
                    ["Main"] = entities.Switch.Sonoff100238104e1,
                }
            ),
            ContactSensor: entities.BinarySensor.ContactSensorDoor,
            LaptopControl: new(
                entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage,
                entities.Switch.Sonoff1002380fe51,
                entities.Button.Thinkpadt14WakeOnWlan,
                entities.Sensor.Thinkpadt14Sessionstate,
                entities.Switch.Laptop,
                entities.Button.Thinkpadt14Lock
            )
        );

    public Area LivingRoom { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.LivingRoomPresenceSensors,
                entities.Button.SalaMotionSensorRestartEsp32,
                entities.Number.SalaMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.SalaMotionSensor, entities.Light.SalaLightsGroup),
            new FanControl(
                entities.Switch.SalaFanAutomation,
                new Dictionary<string, SwitchEntity>
                {
                    ["CeilingFan"] = entities.Switch.CeilingFan,
                    ["StandFan"] = entities.Switch.Sonoff10023810231,
                    ["ExhaustFan"] = entities.Switch.Cozylife955f,
                }
            ),
            MediaPlayer: new(
                entities.MediaPlayer.Tcl65c755,
                entities.Switch.TvAutomation,
                entities.Light.TvBacklight3Lite
            ),
            ContactSensor: entities.BinarySensor.DoorWrapper,
            AirPurifierControl: new(
                entities.Switch.CleanAir,
                entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch,
                entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34,
                entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus
            )
        );

    public Area Kitchen { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.KitchenMotionSensors,
                entities.Button.KitchenMotionSensorRestartEsp32,
                entities.Number.KitchenMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.KitchenMotionSensor, entities.Light.RgbLightStrip),
            CookingControl: new(
                entities.Sensor.RiceCookerPower,
                entities.Switch.RiceCookerSocket1,
                entities.Sensor.CareliSg593061393Maf05aStatusP21,
                entities.Button.InductionCookerPower,
                entities.Sensor.SmartPlug3SonoffS31Power,
                entities.Switch.CookingAutomation
            )
        );

    public Area Pantry { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.PantryMotionSensors,
                entities.Button.PantryMotionSensorRestartEsp32,
                entities.Number.PantryMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.PantryMotionSensor, entities.Light.PantryLights)
        );

    public Area Bathroom { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.BathroomPresenceSensors,
                entities.Button.BathroomMotionSensorRestart,
                entities.Number.BathroomMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.BathroomMotionSensor, entities.Light.BathroomLights)
        );

    public Area Desk { get; } =
        new(
            new MotionControl(
                entities.BinarySensor.DeskMotionSensorSmartPresence,
                entities.Button.DeskMotionSensorRestartEsp32,
                entities.Number.DeskMotionSensorStillTargetDelay
            ),
            new LightControl(entities.Switch.LgTvMotionSensor, entities.Light.LgDisplay),
            MediaPlayer: new(entities.MediaPlayer.LgWebosSmartTv, entities.Switch.LgTvMotionSensor)
        );
}

public record MotionControl(BinarySensorEntity Sensor, ButtonEntity Restart, NumberEntity Timer)
{
    public static implicit operator BinarySensorEntity(MotionControl control) => control.Sensor;

    public static implicit operator ButtonEntity(MotionControl control) => control.Restart;

    public static implicit operator NumberEntity(MotionControl control) => control.Timer;
}

public record LightControl(SwitchEntity Automation, LightEntity Light)
{
    public static implicit operator SwitchEntity(LightControl control) => control.Automation;

    public static implicit operator LightEntity(LightControl control) => control.Light;
}

public record FanControl(SwitchEntity Automation, Dictionary<string, SwitchEntity> Fans)
{
    public SwitchEntity this[string key] => Fans[key];

    public static implicit operator SwitchEntity(FanControl control) => control.Fans.First().Value;
}

public record LaptopControl(
    NumericSensorEntity Level,
    SwitchEntity PowerPlug,
    ButtonEntity WakeOnLanButton,
    SensorEntity Session,
    SwitchEntity VirtualSwitch,
    ButtonEntity Lock
)
{
    public static implicit operator NumericSensorEntity(LaptopControl control) => control.Level;

    public static implicit operator SensorEntity(LaptopControl control) => control.Session;
}

public record AirPurifierControl(
    SwitchEntity Automation,
    SwitchEntity Fan,
    NumericSensorEntity Pm25Sensor,
    SwitchEntity LedStatus
);

public record MediaControl(
    MediaPlayerEntity MediaPlayer,
    SwitchEntity Automation,
    LightEntity? Backlight = null
)
{
    public static implicit operator MediaPlayerEntity(MediaControl control) => control.MediaPlayer;

    public static implicit operator SwitchEntity(MediaControl control) => control.Automation;
}

public record CookingControl(
    NumericSensorEntity RiceCookerPower,
    SwitchEntity RiceCookerSwitch,
    SensorEntity AirFryerStatus,
    ButtonEntity InductionTurnOff,
    NumericSensorEntity InductionPower,
    SwitchEntity Automation
)
{
    public static implicit operator SwitchEntity(CookingControl control) => control.Automation;
}

public class GlobalEntities(Entities entities)
{
    public BinarySensorEntity HouseOccupancy => entities.BinarySensor.House;
}

public record Area(
    MotionControl MotionControl,
    LightControl LightControl,
    FanControl? FanControl = null,
    BinarySensorEntity? ContactSensor = null,
    LaptopControl? LaptopControl = null,
    AirPurifierControl? AirPurifierControl = null,
    MediaControl? MediaPlayer = null,
    CookingControl? CookingControl = null
);
