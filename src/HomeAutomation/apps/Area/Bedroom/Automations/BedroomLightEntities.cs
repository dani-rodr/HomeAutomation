using CommonArea = HomeAutomation.apps.Common.Containers.Area;
using CommonDevices = HomeAutomation.apps.Common.Containers.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomLightEntities(CommonDevices devices) : IBedroomLightEntities
{
    private readonly CommonArea _area = devices.Bedroom;

    public SwitchEntity MasterSwitch => _area.LightControl;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public LightEntity Light => _area.LightControl;
    public NumberEntity SensorDelay => _area.MotionControl;
    public SwitchEntity RightSideEmptySwitch => _area.ExtraControl!.RightSideEmptySwitch!;
    public SwitchEntity LeftSideFanSwitch => _area.FanControl!;
    public ButtonEntity Restart => _area.MotionControl;
}
