using HomeAutomation.apps.Area.Bedroom.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations.Entities;

public class LightEntities(BedroomDevices devices) : IBedroomLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Lights;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public SwitchEntity RightSideEmptySwitch => devices.RightSideEmptySwitch;
    public SwitchEntity LeftSideFanSwitch => devices.MainFan;
    public ButtonEntity Restart => devices.Restart;
}
