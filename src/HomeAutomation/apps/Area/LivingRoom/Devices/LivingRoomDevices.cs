namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomDevices(Entities entities)
{
    public BinarySensorEntity MotionSensor { get; } = entities.BinarySensor.LivingRoomPresenceSensors;
    public BinarySensorEntity SecondaryMotionSensor { get; } =
        entities.BinarySensor.SalaMotionSensorSmartPresence;
    public ButtonEntity Restart { get; } = entities.Button.SalaMotionSensorRestartEsp32;
    public NumberEntity SensorDelay { get; } = entities.Number.SalaMotionSensorStillTargetDelay;
    public SwitchEntity LightAutomation { get; } = entities.Switch.SalaMotionSensor;
    public LightEntity Lights { get; } = entities.Light.SalaLightsGroup;
    public SwitchEntity FanAutomation { get; } = entities.Switch.SalaFanAutomation;
    public SwitchEntity CeilingFan { get; } = entities.Switch.CeilingFan;
    public SwitchEntity StandFan { get; } = entities.Switch.Sonoff10023810231;
    public SwitchEntity ExhaustFan { get; } = entities.Switch.Cozylife955f;
    public MediaPlayerEntity TclTv { get; } = entities.MediaPlayer.Tcl65c755;
    public BinarySensorEntity LivingRoomDoor { get; } = entities.BinarySensor.DoorWrapper;
    public BinarySensorEntity BedroomDoor { get; } = entities.BinarySensor.ContactSensorDoor;
    public BinarySensorEntity BedroomMotionSensor { get; } = entities.BinarySensor.BedroomPresenceSensors;
    public BinarySensorEntity KitchenMotionSensor { get; } = entities.BinarySensor.KitchenMotionSensors;
    public SwitchEntity KitchenMotionAutomation { get; } = entities.Switch.KitchenMotionSensor;
    public LightEntity PantryLights { get; } = entities.Light.PantryLights;
    public SwitchEntity PantryMotionAutomation { get; } = entities.Switch.PantryMotionSensor;
    public BinarySensorEntity PantryMotionSensor { get; } = entities.BinarySensor.PantryMotionSensors;
    public SwitchEntity CleanAirAutomation { get; } = entities.Switch.CleanAir;
    public SwitchEntity AirPurifierFan { get; } =
        entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch;
    public NumericSensorEntity Pm25Sensor { get; } = entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    public SwitchEntity LedStatus { get; } =
        entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;
    public SwitchEntity SupportingFan { get; } = entities.Switch.Sonoff10023810231;
}
