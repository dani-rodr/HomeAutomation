namespace HomeAutomation.apps.Common.Services;

public class LaptopBatteryHandler(IBatteryHandlerEntities entities) : IBatteryHandler
{
    private const int HIGH_BATTERY = 80;
    private const int MEDIUM_BATTERY = 50;
    private const int LOW_BATTERY = 20;
    private readonly NumericSensorEntity _level = entities.Level;
    private readonly SwitchEntity _power = entities.Power;
    public double BatteryLevel => _level?.State ?? _lastBatteryPct;
    private double _lastBatteryPct;

    private CancellationTokenSource? _shutdownCts;

    public IDisposable StartMonitoring() => _level.StateChanges().Subscribe(e => ApplyChargingLogic());

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
            _power.TurnOff();
            return;
        }

        _power.TurnOn();
        try
        {
            await Task.Delay(TimeSpan.FromHours(1), _shutdownCts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        _power.TurnOff();
    }

    private void ApplyChargingLogic(bool forceCharge = false)
    {
        if (_level.State.HasValue)
        {
            _lastBatteryPct = _level.State.Value;
        }
        if (BatteryLevel >= HIGH_BATTERY)
        {
            _power.TurnOff();
        }
        else if (BatteryLevel <= LOW_BATTERY || forceCharge)
        {
            _power.TurnOn();
        }
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
