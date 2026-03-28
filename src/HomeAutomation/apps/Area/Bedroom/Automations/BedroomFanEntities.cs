using CommonArea = HomeAutomation.apps.Common.Containers.Area;
using CommonDevices = HomeAutomation.apps.Common.Containers.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomFanEntities(CommonDevices devices) : IBedroomFanEntities
{
    private readonly CommonArea _area = devices.Bedroom;

    public SwitchEntity MasterSwitch => _area.FanControl!.Automation;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public IEnumerable<SwitchEntity> Fans => _area.FanControl!.Fans.Values;
}
