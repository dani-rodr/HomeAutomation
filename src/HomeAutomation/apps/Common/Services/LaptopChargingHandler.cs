using System.Reactive.Disposables;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class LaptopChargingHandler(
    IChargingHandlerEntities entities,
    ILogger<LaptopChargingHandler> logger
) : ChargingHandler(entities), ILaptopChargingHandler
{
    private const int MEDIUM_BATTERY = 50;
    private IDisposable? _powerOffTimer;
    private readonly IScheduler _scheduler = SchedulerProvider.Current;

    public override IDisposable StartMonitoring()
    {
        logger.LogInformation(
            "Starting laptop charging monitoring. Power will be turned off initially."
        );
        Power.TurnOff();

        var baseMonitoring = base.StartMonitoring();
        var inactiveSchedules = Power
            .OnTurnedOff(new(Hours: 12, CheckImmediately: true))
            .Subscribe(_ => StartScheduledCharge(hours: 1));
        logger.LogInformation("Setting up weekend and Monday morning charging schedules.");
        var weekendChargingSchedules = new CompositeDisposable(
            _scheduler.ScheduleCron("0 22 * * 5", () => StartScheduledCharge(1)), // Fri 22:00
            _scheduler.ScheduleCron("0 8 * * 6", () => StartScheduledCharge(1)), // Sat 08:00
            _scheduler.ScheduleCron("0 18 * * 6", () => StartScheduledCharge(1)), // Sat 18:00
            _scheduler.ScheduleCron("0 4 * * 0", () => StartScheduledCharge(1)), // Sun 04:00
            _scheduler.ScheduleCron("0 14 * * 0", () => StartScheduledCharge(1)), // Sun 14:00
            _scheduler.ScheduleCron("0 0 * * 1", () => StartScheduledCharge(1)), // Mon 00:00
            _scheduler.ScheduleCron("0 6 * * 1", () => StartScheduledCharge(1)) // Mon 06:00
        );

        return new CompositeDisposable(baseMonitoring, weekendChargingSchedules, inactiveSchedules);
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
            .Timer(TimeSpan.FromHours(1), _scheduler)
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

        _scheduler.Schedule(
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
