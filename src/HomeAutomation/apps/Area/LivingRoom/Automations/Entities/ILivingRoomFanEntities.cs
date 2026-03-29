namespace HomeAutomation.apps.Area.LivingRoom.Automations.Entities;

public interface ILivingRoomFanEntities : IFanAutomationEntities
{
    BinarySensorEntity BedroomMotionSensor { get; }
    SwitchEntity ExhaustFan { get; }
}
