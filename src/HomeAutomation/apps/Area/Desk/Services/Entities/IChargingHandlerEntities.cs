namespace HomeAutomation.apps.Area.Desk.Services.Entities;

public interface IChargingHandlerEntities
{
    NumericSensorEntity Level { get; }
    SwitchEntity Power { get; }
}
