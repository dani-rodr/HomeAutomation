using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class LivingRoomFanEntities(LivingRoomFanDevices devices) : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => devices.FanAutomation;
    public BinarySensorEntity MotionSensor => devices.SecondaryMotionSensor;
    public IEnumerable<SwitchEntity> Fans => [devices.CeilingFan, devices.StandFan];
    public BinarySensorEntity BedroomMotionSensor => devices.BedroomMotionSensor;
    public SwitchEntity ExhaustFan => devices.ExhaustFan;
}
