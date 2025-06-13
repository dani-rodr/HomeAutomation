namespace HomeAutomation.apps.Common.Containers;

public interface IAirQualityEntities
{
    SwitchEntity CleanAirSwitch { get; }
    BinarySensorEntity PresenceSensor { get; }
    SwitchEntity AirPurifierFan { get; }
    SwitchEntity SupportingFan { get; }
    NumericSensorEntity Pm25Sensor { get; }
    SwitchEntity LedStatus { get; }
}

public class AirQualityEntities(Entities entities, ILivingRoomSharedEntities sharedEntities) : IAirQualityEntities
{
    public SwitchEntity CleanAirSwitch => entities.Switch.CleanAir;
    public BinarySensorEntity PresenceSensor => sharedEntities.MotionSensor;
    public SwitchEntity AirPurifierFan => entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch;
    public SwitchEntity SupportingFan => sharedEntities.StandFan;
    public NumericSensorEntity Pm25Sensor => entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    public SwitchEntity LedStatus => entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;
}
