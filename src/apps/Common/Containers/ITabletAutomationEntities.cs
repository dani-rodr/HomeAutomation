namespace HomeAutomation.apps.Common.Containers;

public interface ITabletAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    LightEntity TabletScreen { get; }
    BinarySensorEntity TabletActive { get; }
}

public class LivingRoomTabletEntities(Entities entities, ILivingRoomSharedEntities sharedEntities)
    : ITabletAutomationEntities
{
    public SwitchEntity MasterSwitch => sharedEntities.MotionSensorSwitch;
    public BinarySensorEntity MotionSensor => sharedEntities.MotionSensor;
    public LightEntity TabletScreen => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
}
