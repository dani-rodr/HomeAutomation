using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Common.Devices;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomClimateEntities(BedroomDevices devices, GlobalDevices globalDevices) : IClimateEntities
{
    public SwitchEntity MasterSwitch => devices.ClimateAutomation;
    public ClimateEntity AirConditioner => devices.AirConditioner;
    public BinarySensorEntity MotionSensor => devices.MotionSensor;
    public BinarySensorEntity Door => devices.Door;
    public BinarySensorEntity HouseMotionSensor => globalDevices.HouseMotionSensor;
    public ButtonEntity AcFanModeToggle => devices.AcFanModeToggle;
    public SwitchEntity Fan => devices.MainFan;
    public InputBooleanEntity PowerSavingMode => devices.PowerSavingMode;
}
