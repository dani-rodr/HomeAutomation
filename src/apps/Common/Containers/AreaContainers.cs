using System.Linq;

namespace HomeAutomation.apps.Common.Containers;

public record Area(
    MotionControl MotionControl,
    LightControl LightControl,
    MotionControl? SecondaryMotionControl = null,
    FanControl? FanControl = null,
    BinarySensorEntity? ContactSensor = null,
    LaptopControl? LaptopControl = null,
    AirPurifierControl? AirPurifierControl = null,
    MediaControl? MediaControl = null,
    CookingControl? CookingControl = null,
    ClimateControl? ClimateControl = null,
    PcControl? PcControl = null,
    MotionLightControl? MotionLightControl = null,
    WeatherControl? WeatherControl = null,
    LockControl? LockControl = null,
    PeopleControl? PeopleControl = null,
    ExtraControl? ExtraControl = null
);

public record PersonControlSet(PersonEntity Person, ButtonEntity Toggle);

public record PeopleControl(
    PersonControlSet Daniel,
    PersonControlSet Athena,
    CounterEntity Counter
);

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
    ButtonEntity Lock,
    ButtonEntity Sleep,
    InputBooleanEntity ProjectNationWeek
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

public record ClimateControl(SwitchEntity Automation, ClimateEntity Ac, ButtonEntity FanModeToggle)
{
    public static implicit operator SwitchEntity(ClimateControl control) => control.Automation;

    public static implicit operator ClimateEntity(ClimateControl control) => control.Ac;

    public static implicit operator ButtonEntity(ClimateControl control) => control.FanModeToggle;
}

public record CookingControl(
    NumericSensorEntity RiceCookerPower,
    SwitchEntity RiceCookerSwitch,
    SensorEntity AirFryerStatus,
    ButtonEntity InductionTurnOff,
    NumericSensorEntity InductionPower,
    SwitchEntity Automation,
    TimerEntity AirFryerTimer
)
{
    public static implicit operator SwitchEntity(CookingControl control) => control.Automation;
}

public record MotionLightControl(BinarySensorEntity Sensor, LightEntity Light)
{
    public static implicit operator BinarySensorEntity(MotionLightControl control) =>
        control.Sensor;

    public static implicit operator LightEntity(MotionLightControl control) => control.Light;
}

public record WeatherControl(
    SensorEntity SunRising,
    SensorEntity SunSetting,
    SensorEntity SunMidnight,
    WeatherEntity Weather,
    InputBooleanEntity PowerSavingMode
);

public record LockControl(
    SwitchEntity Automation,
    LockEntity Lock,
    BinarySensorEntity Door,
    BinarySensorEntity HouseStatus,
    SwitchEntity Flytrap
);

public record PcControl(SwitchEntity Power, InputButtonEntity RemotePcButton);

public record ExtraControl(
    SwitchEntity? RightSideEmptySwitch = null,
    BinarySensorEntity? HouseOccupancy = null,
    BinarySensorEntity? PowerPlug = null
);
