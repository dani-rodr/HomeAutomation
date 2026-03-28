namespace HomeAutomation.apps.Area.LivingRoom.Devices;

public class LivingRoomTabletEntities(LivingRoomDevices devices) : ITabletEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.TabletLight;
    public BinarySensorEntity TabletActive => devices.TabletActive;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public ButtonEntity Restart => devices.Restart;
}
