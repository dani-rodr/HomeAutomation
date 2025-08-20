namespace HomeAutomation.apps.Common.Containers;

public class Devices(Entities entities)
{
    public Area Global { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.House,
                entities.Button.RestartEsp32,
                entities.Number.SalaMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.MotionSensors, entities.Light.Lights),
            WeatherControl: new(
                entities.Sensor.SunNextRising,
                entities.Sensor.SunNextSetting,
                entities.Sensor.SunNextMidnight,
                entities.Weather.Home,
                entities.InputBoolean.AcPowerSavingMode
            ),
            PeopleControl: new(
                Daniel: new(
                    entities.Person.DanielRodriguez,
                    entities.Button.ManualTrackerButtonDaniel
                ),
                Athena: new(entities.Person.AthenaBezos, entities.Button.ManualTrackerButtonAthena),
                Counter: entities.Counter.People
            )
        );
    public Area Bedroom { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.BedroomPresenceSensors,
                entities.Button.BedroomMotionSensorRestartEsp32,
                entities.Number.BedroomMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.BedroomMotionSensor, entities.Light.BedLights),
            FanControl: new(
                entities.Switch.BedroomFanAutomation,
                new() { ["Main"] = entities.Switch.Sonoff100238104e1 }
            ),
            ContactSensor: entities.BinarySensor.ContactSensorDoor,
            ClimateControl: new(
                entities.Switch.AcAutomation,
                entities.Climate.Ac,
                entities.Button.AcFanModeToggle
            ),
            ExtraControl: new(RightSideEmptySwitch: entities.Switch.Sonoff1002352c401)
        );

    public Area LivingRoom { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.LivingRoomPresenceSensors,
                entities.Button.SalaMotionSensorRestartEsp32,
                entities.Number.SalaMotionSensorStillTargetDelay
            ),
            SecondaryMotionControl: new(
                entities.BinarySensor.SalaMotionSensorSmartPresence,
                entities.Button.SalaMotionSensorRestartEsp32,
                entities.Number.SalaMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.SalaMotionSensor, entities.Light.SalaLightsGroup),
            FanControl: new(
                entities.Switch.SalaFanAutomation,
                new()
                {
                    ["CeilingFan"] = entities.Switch.CeilingFan,
                    ["StandFan"] = entities.Switch.Sonoff10023810231,
                },
                ExhaustFan: entities.Switch.Cozylife955f
            ),
            MediaControl: new(
                entities.MediaPlayer.Tcl65c755,
                entities.Switch.TvAutomation,
                entities.Light.TvBacklight3Lite
            ),
            ContactSensor: entities.BinarySensor.DoorWrapper,
            AirPurifierControl: new(
                entities.Switch.CleanAir,
                entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch,
                entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34,
                entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus,
                SupportingFan: entities.Switch.Sonoff10023810231
            ),
            MotionLightControl: new(entities.BinarySensor.Mipad, entities.Light.MipadScreen),
            LockControl: new(
                entities.Switch.LockAutomation,
                entities.Lock.LockWrapper,
                entities.BinarySensor.DoorWrapper,
                entities.BinarySensor.House,
                entities.Switch.Flytrap
            )
        );

    public Area Kitchen { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.KitchenMotionSensors,
                entities.Button.KitchenMotionSensorRestartEsp32,
                entities.Number.KitchenMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.KitchenMotionSensor, entities.Light.RgbLightStrip),
            CookingControl: new(
                entities.Sensor.RiceCookerPower,
                entities.Switch.RiceCookerSocket1,
                entities.Sensor.XiaomiSmartAirFryerPro4lAirFryerOperatingStatus,
                entities.Button.InductionCookerPower,
                entities.Sensor.SmartPlug3SonoffS31Power,
                entities.Switch.CookingAutomation,
                entities.Timer.AirFryer
            ),
            ExtraControl: new(PowerPlug: entities.BinarySensor.SmartPlug3PowerExceedsThreshold)
        );

    public Area Pantry { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.PantryMotionSensors,
                entities.Button.PantryMotionSensorRestartEsp32,
                entities.Number.PantryMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.PantryMotionSensor, entities.Light.PantryLights),
            MotionLightControl: new(
                entities.BinarySensor.BedroomMotionSensorMiScalePresence,
                entities.Light.ControllerRgbDf1c0d
            )
        );

    public Area Bathroom { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.BathroomPresenceSensors,
                entities.Button.BathroomMotionSensorRestart,
                entities.Number.BathroomMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.BathroomMotionSensor, entities.Light.BathroomLights)
        );

    public Area Desk { get; } =
        new(
            MotionControl: new(
                entities.BinarySensor.DeskMotionSensorSmartPresence,
                entities.Button.DeskMotionSensorRestartEsp32,
                entities.Number.DeskMotionSensorStillTargetDelay
            ),
            LightControl: new(entities.Switch.LgTvMotionSensor, entities.Light.LgDisplay),
            MediaControl: new(
                entities.MediaPlayer.LgWebosSmartTv,
                entities.Switch.LgTvMotionSensor
            ),
            LaptopControl: new(
                entities.Sensor.Thinkpadt14BatteryChargeRemainingPercentage,
                entities.Switch.Sonoff1002380fe51,
                entities.Button.Thinkpadt14WakeOnWlan,
                entities.Sensor.Thinkpadt14Sessionstate,
                entities.Switch.Laptop,
                entities.Button.Thinkpadt14Lock,
                entities.Button.Thinkpadt14Sleep,
                entities.InputBoolean.ProjectNationWeek
            ),
            PcControl: new(entities.Switch.DanielPc, entities.InputButton.RemotePc)
        );
}
