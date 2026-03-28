using CommonArea = HomeAutomation.apps.Common.Containers.Area;
using CommonDevices = HomeAutomation.apps.Common.Containers.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomClimateEntities(CommonDevices devices) : IClimateEntities
{
    private readonly CommonArea _area = devices.Bedroom;

    public SwitchEntity MasterSwitch => _area.ClimateControl!;
    public ClimateEntity AirConditioner => _area.ClimateControl!;
    public BinarySensorEntity MotionSensor => _area.MotionControl;
    public BinarySensorEntity Door => _area.ContactSensor!;
    public BinarySensorEntity HouseMotionSensor => devices.Global.MotionControl;
    public ButtonEntity AcFanModeToggle => _area.ClimateControl!;
    public SwitchEntity Fan => _area.FanControl!;
    public InputBooleanEntity PowerSavingMode => _area.ClimateControl!;
}
