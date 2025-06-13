namespace HomeAutomation.apps.Common.Containers;

public interface ITabletAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    LightEntity TabletScreen { get; }
    BinarySensorEntity TabletActive { get; }
}

public class LivingRoomTabletEntities(Entities entities, SwitchEntity masterSwitch, BinarySensorEntity motionSensor)
    : ITabletAutomationEntities
{
    public SwitchEntity MasterSwitch => masterSwitch;
    public BinarySensorEntity MotionSensor => motionSensor;
    public LightEntity TabletScreen => entities.Light.MipadScreen;
    public BinarySensorEntity TabletActive => entities.BinarySensor.Mipad;
}
