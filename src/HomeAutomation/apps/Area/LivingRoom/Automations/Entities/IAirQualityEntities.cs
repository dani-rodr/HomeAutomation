namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public interface IAirQualityEntities : IFanAutomationEntities
{
    NumericSensorEntity Pm25Sensor { get; }
    SwitchEntity LedStatus { get; }
    SwitchEntity LivingRoomFanAutomation { get; }
    SwitchEntity SupportingFan { get; }
}
