using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations.Entities;

public class TabletEntities(
    LivingRoomLightDevices devices,
    LivingRoomMediaDevices mediaDevices
) : ITabletEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => mediaDevices.MotionSensor;
    public LightEntity Light => mediaDevices.TabletLight;
    public BinarySensorEntity TabletActive => mediaDevices.TabletActive;
    public NumberEntity SensorDelay => mediaDevices.SensorDelay;
    public ButtonEntity Restart => mediaDevices.Restart;
}
