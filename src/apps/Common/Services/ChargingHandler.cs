namespace HomeAutomation.apps.Common.Services;

public class ChargingHandler(IChargingHandlerEntities entities) : IChargingHandler
{
    private const int HIGH_BATTERY = 80;
    private const int LOW_BATTERY = 20;
    protected readonly NumericSensorEntity Level = entities.Level;
    protected readonly SwitchEntity Power = entities.Power;
    public double BatteryLevel => Level?.State ?? _lastBatteryPct;
    private double _lastBatteryPct;

    public IDisposable StartMonitoring() =>
        Level.StateChanges().Subscribe(e => ApplyChargingLogic());

    protected void ApplyChargingLogic(bool forceCharge = false)
    {
        if (Level.State.HasValue)
        {
            _lastBatteryPct = Level.State.Value;
        }
        if (BatteryLevel >= HIGH_BATTERY)
        {
            Power.TurnOff();
        }
        else if (BatteryLevel <= LOW_BATTERY || forceCharge)
        {
            Power.TurnOn();
        }
    }
}
