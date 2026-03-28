namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomFanDevices(Entities entities)
{
    public SwitchEntity FanAutomation { get; } = entities.Switch.SalaFanAutomation;
    public BinarySensorEntity SecondaryMotionSensor { get; } =
        entities.BinarySensor.SalaMotionSensorSmartPresence;
    public SwitchEntity CeilingFan { get; } = entities.Switch.CeilingFan;
    public SwitchEntity StandFan { get; } = entities.Switch.Sonoff10023810231;
    public SwitchEntity ExhaustFan { get; } = entities.Switch.Cozylife955f;
    public BinarySensorEntity BedroomMotionSensor { get; } =
        entities.BinarySensor.BedroomPresenceSensors;
}
