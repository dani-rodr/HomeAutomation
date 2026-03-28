using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DeskLightEntities(DeskDevices devices) : IDeskLightEntities
{
    public SwitchEntity MasterSwitch => devices.LightAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public LightEntity Light => devices.Display;
    public NumberEntity SensorDelay => devices.SensorDelay;
    public ButtonEntity Restart => devices.Restart;
    public LightEntity SalaLights => devices.SalaLights;
}
