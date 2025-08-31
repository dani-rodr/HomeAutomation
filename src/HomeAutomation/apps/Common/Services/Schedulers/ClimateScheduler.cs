using System.Diagnostics.CodeAnalysis;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services.Schedulers;

public class ClimateScheduler : IClimateScheduler
{
    private readonly IClimateSchedulerEntities _entities;
    private readonly IScheduler _scheduler;
    private readonly IAcTemperatureCalculator _temperatureCalculator;
    private readonly ILogger _logger;

    public ClimateScheduler(
        IClimateSchedulerEntities entities,
        IAcTemperatureCalculator temperatureCalculator,
        ILogger<ClimateScheduler> logger
    )
    {
        _entities = entities;
        _scheduler = SchedulerProvider.Current;
        _temperatureCalculator = temperatureCalculator;
        _logger = logger;

        LogCurrentAcScheduleSettings();
    }

    public IEnumerable<IDisposable> GetSchedules(Action action)
    {
        foreach (var (timeBlock, setting) in GetCurrentAcScheduleSettings())
        {
            if (!setting.IsValidHourRange())
            {
                _logger.LogWarning(
                    "Invalid HourStart {HourStart} for TimeBlock {TimeBlock}. Skipping schedule.",
                    setting.HourStart,
                    timeBlock
                );
                continue;
            }
            // Fire 1 minute after the hour to avoid boundary ambiguity
            // At exact boundary times (5:00 AM), time block evaluation can be ambiguous
            // Running at 5:05 AM ensures we're clearly within the Sunrise period
            string cron = $"5 {setting.HourStart} * * *";
            _logger.LogInformation(
                "Scheduling action for TimeBlock {TimeBlock} at cron '{Cron}'",
                timeBlock,
                cron
            );
            yield return _scheduler.ScheduleCron(cron, action);
        }
    }

    public IDisposable GetResetSchedule() =>
        _scheduler.ScheduleCron(
            "0 0 * * *",
            () =>
            {
                _cachedAcSettings = null;
                LogCurrentAcScheduleSettings();
            }
        );

    public bool TryGetSetting(TimeBlock timeBlock, [NotNullWhen(true)] out AcSettings? setting)
    {
        var settings = GetCurrentAcScheduleSettings();
        return settings.TryGetValue(timeBlock, out setting);
    }

    public int CalculateTemperature(AcSettings settings, bool isOccupied, bool isDoorOpen) =>
        _temperatureCalculator.CalculateTemperature(settings, isOccupied, isDoorOpen);

    public TimeBlock? FindCurrentTimeBlock()
    {
        var currentTime = _scheduler.Now.LocalDateTime;
        _logger.LogDebug("Finding time block for current time: {CurrentTime}", currentTime);

        var settings = GetCurrentAcScheduleSettings();

        // Find the first time block that matches current time
        // Let TimeRange.IsTimeInBetween handle all the overnight range logic
        foreach (var (timeBlock, setting) in settings)
        {
            if (
                TimeRange.IsTimeInBetween(currentTime.TimeOfDay, setting.HourStart, setting.HourEnd)
            )
            {
                _logger.LogDebug(
                    "Found matching time block: {TimeBlock} (range: {StartHour}-{EndHour})",
                    timeBlock,
                    setting.HourStart,
                    setting.HourEnd
                );
                return timeBlock;
            }

            _logger.LogDebug(
                "Time block {TimeBlock} (range: {StartHour}-{EndHour}) does not match current time {CurrentTime}",
                timeBlock,
                setting.HourStart,
                setting.HourEnd,
                currentTime.TimeOfDay
            );
        }

        _logger.LogDebug("No time block found for current hour {CurrentHour}", currentTime.Hour);
        return null;
    }

    private Dictionary<TimeBlock, AcSettings>? _cachedAcSettings;

    private Dictionary<TimeBlock, AcSettings> GetCurrentAcScheduleSettings()
    {
        if (_cachedAcSettings != null)
        {
            return _cachedAcSettings;
        }

        _cachedAcSettings = new()
        {
            [TimeBlock.Sunrise] = new(
                NormalTemp: 25,
                PowerSavingTemp: 27,
                CoolTemp: 24,
                PassiveTemp: 27,
                Mode: HaEntityStates.COOL,
                ActivateFan: true,
                HourStart: _entities.SunRising.LocalHour(),
                HourEnd: _entities.SunSetting.LocalHour()
            ),

            [TimeBlock.Sunset] = new(
                NormalTemp: 25,
                PowerSavingTemp: 27,
                CoolTemp: 23,
                PassiveTemp: 27,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: _entities.SunSetting.LocalHour(),
                HourEnd: _entities.SunMidnight.LocalHour()
            ),

            [TimeBlock.Midnight] = new(
                NormalTemp: 24,
                PowerSavingTemp: 25,
                CoolTemp: 22,
                PassiveTemp: 25,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: _entities.SunMidnight.LocalHour(),
                HourEnd: _entities.SunRising.LocalHour()
            ),
        };

        return _cachedAcSettings;
    }

    private void LogCurrentAcScheduleSettings()
    {
        _logger.LogDebug(
            "AC schedule settings initialized based on current sun sensor values. HourStart and HourEnd may vary daily depending on sunrise, sunset, and midnight times."
        );
        foreach (var kvp in GetCurrentAcScheduleSettings())
        {
            var setting = kvp.Value;
            _logger.LogDebug(
                "TimeBlock {TimeBlock}: NormalTemp={NormalTemp},"
                    + " PowerSavingTemp={PowerSavingTemp}, CoolTemp={CoolTemp},"
                    + " PassiveTemp={PassiveTemp}, Mode={Mode}, ActivateFan={ActivateFan},"
                    + " HourStart={HourStart}, HourEnd={HourEnd}",
                kvp.Key,
                setting.NormalTemp,
                setting.PowerSavingTemp,
                setting.CoolTemp,
                setting.PassiveTemp,
                setting.Mode,
                setting.ActivateFan,
                setting.HourStart,
                setting.HourEnd
            );
        }
    }
}
