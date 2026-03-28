namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public interface ILivingRoomFanEntities : IFanAutomationEntities
{
    BinarySensorEntity BedroomMotionSensor { get; }
    SwitchEntity ExhaustFan { get; }
}
