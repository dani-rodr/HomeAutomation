using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityEntities(LivingRoomAirQualityDevices devices) : IAirQualityEntities
{
    public SwitchEntity MasterSwitch => devices.CleanAirAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public IEnumerable<SwitchEntity> Fans => [devices.AirPurifierFan, devices.StandFan];
    public NumericSensorEntity Pm25Sensor => devices.Pm25Sensor;
    public SwitchEntity LedStatus => devices.LedStatus;
    public SwitchEntity LivingRoomFanAutomation => devices.FanAutomation;
    public SwitchEntity SupportingFan => devices.SupportingFan;
}
