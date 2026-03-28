namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomAirQualityDevices(HomeAssistantGenerated.Entities entities)
{
    public SwitchEntity CleanAirAutomation { get; } = entities.Switch.CleanAir;
    public BinarySensorEntity MotionSensor { get; } =
        entities.BinarySensor.LivingRoomPresenceSensors;
    public SwitchEntity AirPurifierFan { get; } =
        entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch;
    public SwitchEntity StandFan { get; } = entities.Switch.Sonoff10023810231;
    public SwitchEntity SupportingFan { get; } = entities.Switch.Sonoff10023810231;
    public NumericSensorEntity Pm25Sensor { get; } =
        entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    public SwitchEntity LedStatus { get; } =
        entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;
    public SwitchEntity FanAutomation { get; } = entities.Switch.SalaFanAutomation;
    public SwitchEntity LivingRoomFanAutomation { get; } = entities.Switch.SalaFanAutomation;
}
