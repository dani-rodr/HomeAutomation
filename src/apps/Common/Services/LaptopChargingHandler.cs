using System.Reactive.Disposables;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class LaptopChargingHandler(
    IChargingHandlerEntities entities,
    IScheduler scheduler,
    ILogger<LaptopChargingHandler> logger
) : ChargingHandler(entities), ILaptopChargingHandler
{
    private const int MEDIUM_BATTERY = 50;
    private IDisposable? _powerOffTimer;

    public override IDisposable StartMonitoring()
    {
        logger.LogInformation(
            "Starting laptop charging monitoring. Power will be turned off initially."
        );
        Power.TurnOff();

        var baseMonitoring = base.StartMonitoring();

        logger.LogInformation("Setting up weekend and Monday morning charging schedules.");
        var weekendCharging = new CompositeDisposable(
            scheduler.ScheduleCron("0 22 * * 5", () => StartScheduledCharge(1)), // Friday 10 PM
            scheduler.ScheduleCron("0 10 * * 6", () => StartScheduledCharge(1)), // Saturday 10 AM
            scheduler.ScheduleCron("0 18 * * 6", () => StartScheduledCharge(1)), // Saturday 6 PM
            scheduler.ScheduleCron("0 10 * * 0", () => StartScheduledCharge(1)), // Sunday 10 AM
            scheduler.ScheduleCron("0 18 * * 0", () => StartScheduledCharge(1)), // Sunday 6 PM
            scheduler.ScheduleCron("0 06 * * 1", () => StartScheduledCharge(1)) // Monday 6 AM
        );

        return new CompositeDisposable(baseMonitoring, weekendCharging);
    }

    public void HandleLaptopTurnedOn()
    {
        logger.LogInformation("Laptop turned on. Cancelling any pending power-off timer.");
        _powerOffTimer?.Dispose();
        _powerOffTimer = null;

        logger.LogInformation("Applying charging logic with forceCharge = true.");
        ApplyChargingLogic(forceCharge: true);
    }

    public void HandleLaptopTurnedOff()
    {
        if (BatteryLevel > MEDIUM_BATTERY)
        {
            logger.LogInformation(
                "Laptop turned off with battery > {BatteryLevel}%. Turning power off.",
                BatteryLevel
            );
            Power.TurnOff();
            return;
        }

        logger.LogInformation(
            "Laptop turned off with battery <= {BatteryLevel}%. Starting 1-hour charging session.",
            BatteryLevel
        );
        Power.TurnOn();

        logger.LogInformation("Scheduling power off after 1 hour.");
        _powerOffTimer?.Dispose();
        _powerOffTimer = Observable
            .Timer(TimeSpan.FromHours(1), scheduler)
            .Subscribe(_ =>
            {
                logger.LogInformation("1-hour timer expired. Turning power off.");
                Power.TurnOff();
            });
    }

    private void StartScheduledCharge(int hours)
    {
        logger.LogInformation("Starting scheduled charge for {Hours} hour(s).", hours);
        Power.TurnOn();

        scheduler.Schedule(
            TimeSpan.FromHours(hours),
            () =>
            {
                logger.LogInformation(
                    "Scheduled charge ended after {Hours} hour(s). Turning power off.",
                    hours
                );
                Power.TurnOff();
            }
        );
    }

    public void Dispose()
    {
        logger.LogInformation("Disposing laptop charging handler and any active timers.");
        _powerOffTimer?.Dispose();
        _powerOffTimer = null;
        GC.SuppressFinalize(this);
    }
}
