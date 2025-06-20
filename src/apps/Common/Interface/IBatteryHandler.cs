namespace HomeAutomation.apps.Common.Interface;

public interface IBatteryHandler : IDisposable
{
    IDisposable StartMonitoring();
    void HandleLaptopTurnedOn();
    Task HandleLaptopTurnedOffAsync();
}
