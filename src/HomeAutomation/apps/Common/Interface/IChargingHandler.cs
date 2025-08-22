namespace HomeAutomation.apps.Common.Interface;

public interface IChargingHandler
{
    IDisposable StartMonitoring();
}

public interface ILaptopChargingHandler : IChargingHandler, IDisposable
{
    void HandleLaptopTurnedOn();
    void HandleLaptopTurnedOff();
}
