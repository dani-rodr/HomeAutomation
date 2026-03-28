using HomeAutomation.apps.Area.Bedroom.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomFanEntities(BedroomDevices devices) : IBedroomFanEntities
{
    public SwitchEntity MasterSwitch => devices.FanAutomation;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public IEnumerable<SwitchEntity> Fans => [devices.MainFan];
}
