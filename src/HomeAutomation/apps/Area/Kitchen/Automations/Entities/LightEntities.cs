using HomeAutomation.apps.Area.Kitchen.Devices;

namespace HomeAutomation.apps.Area.Kitchen.Automations.Entities;

public class LightEntities(KitchenDevices devices) : IKitchenLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Lights;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public BinarySensorEntity PowerPlug => devices.PowerPlug;
    public ButtonEntity Restart => devices.Restart;
}
