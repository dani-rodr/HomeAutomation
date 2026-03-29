using HomeAutomation.apps.Area.Bathroom.Devices;

namespace HomeAutomation.apps.Area.Bathroom.Automations.Entities;

public class LightEntities(BathroomDevices devices) : IBathroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Lights;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public ButtonEntity Restart => devices.Restart;
}
