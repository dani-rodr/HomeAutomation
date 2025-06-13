namespace HomeAutomation.apps.Common.Containers;

public interface IFanAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    IEnumerable<SwitchEntity> Fans { get; }
}

public class BedroomFanEntities(Entities entities) : IFanAutomationEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BedroomMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BedroomPresenceSensors;
    public IEnumerable<SwitchEntity> Fans => [entities.Switch.Sonoff100238104e1];
}

public interface ILivingRoomFanEntities : IFanAutomationEntities
{
    BinarySensorEntity BedroomPresenceSensor { get; }
}

public class LivingRoomFanEntities(Entities entities, ILivingRoomSharedEntities sharedEntities) : ILivingRoomFanEntities
{
    public SwitchEntity MasterSwitch => sharedEntities.MotionSensorSwitch;
    public BinarySensorEntity MotionSensor => sharedEntities.MotionSensor;
    public IEnumerable<SwitchEntity> Fans =>
        [entities.Switch.CeilingFan, sharedEntities.StandFan, entities.Switch.Cozylife955f];
    public BinarySensorEntity BedroomPresenceSensor => entities.BinarySensor.BedroomPresenceSensors;
}
