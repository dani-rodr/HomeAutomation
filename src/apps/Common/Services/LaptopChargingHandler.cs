using System.Reactive.Threading.Tasks;

namespace HomeAutomation.apps.Common.Services;

public class LaptopChargingHandler(IChargingHandlerEntities entities, IScheduler scheduler)
    : ChargingHandler(entities),
        ILaptopChargingHandler
{
    private const int MEDIUM_BATTERY = 50;

    private CancellationTokenSource? _shutdownCts;

    public override IDisposable StartMonitoring()
    {
        Power.TurnOff();
        return base.StartMonitoring();
    }

    public void HandleLaptopTurnedOn()
    {
        CleanShutdownCts();

        ApplyChargingLogic(forceCharge: true);
    }

    public async Task HandleLaptopTurnedOffAsync()
    {
        CleanShutdownCts();
        _shutdownCts = new();
        if (BatteryLevel > MEDIUM_BATTERY)
        {
            Power.TurnOff();
            return;
        }

        Power.TurnOn();
        try
        {
            var token = _shutdownCts.Token;
            await Observable
                .Timer(TimeSpan.FromHours(1), scheduler)
                .TakeUntil(token.AsObservable())
                .ToTask(token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        Power.TurnOff();
    }

    private void CleanShutdownCts()
    {
        _shutdownCts?.Cancel();
        _shutdownCts?.Dispose();
        _shutdownCts = null;
    }

    public void Dispose()
    {
        CleanShutdownCts();
        GC.SuppressFinalize(this);
    }
}
