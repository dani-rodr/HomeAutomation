namespace HomeAutomation.apps.Area.Desk.Devices;

public interface IChargingHandlerEntities
{
    NumericSensorEntity Level { get; }
    SwitchEntity Power { get; }
}
