using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Services.Entities;

public class LaptopChargingHandlerEntities(DeskDevices devices) : IChargingHandlerEntities
{
    public NumericSensorEntity Level => devices.LaptopBatteryLevel;
    public SwitchEntity Power => devices.LaptopPowerPlug;
}
