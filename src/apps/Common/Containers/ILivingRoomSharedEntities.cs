namespace HomeAutomation.apps.Common.Containers;

/// <summary>
/// Shared entities used across multiple LivingRoom automations.
/// Provides single source of truth for commonly referenced entities.
/// </summary>
public interface ILivingRoomSharedEntities
{
    SwitchEntity StandFan { get; }
    SwitchEntity MotionSensorSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
}

public class LivingRoomSharedEntities(Entities entities) : ILivingRoomSharedEntities
{
    public SwitchEntity StandFan => entities.Switch.Sonoff10023810231;
    public SwitchEntity MotionSensorSwitch => entities.Switch.SalaMotionSensor;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.LivingRoomPresenceSensors;
}
