namespace HomeAutomation.apps.Area.Desk.Devices;

public class LaptopChargingHandlerEntities(DeskDevices devices) : IChargingHandlerEntities
{
    public NumericSensorEntity Level => devices.LaptopBatteryLevel;
    public SwitchEntity Power => devices.LaptopPowerPlug;
}
