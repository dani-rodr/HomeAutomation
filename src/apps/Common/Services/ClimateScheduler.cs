using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class ClimateScheduler : IClimateScheduler
{
    private readonly IClimateSchedulerEntities _entities;
    private readonly IScheduler _scheduler;
    private readonly ILogger _logger;

    public ClimateScheduler(
        IClimateSchedulerEntities entities,
        IScheduler scheduler,
        ILogger logger
    )
    {
        _entities = entities;
        _scheduler = scheduler;
        _logger = logger;
        AcScheduleSetting.Initialize(entities.Weather, entities.PowerSavingMode, logger);

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
            string cron = $"0 {setting.HourStart} * * *";
            yield return _scheduler.ScheduleCron(cron, action);
        }
    }

    public IDisposable GetResetSchedule() =>
        _scheduler.ScheduleCron(
            "0 0 * * *",
            () =>
            {
                _cachedAcSettings = null;
            }
        );

    public bool TryGetSetting(
        TimeBlock timeBlock,
        [NotNullWhen(true)] out AcScheduleSetting? setting
    )
    {
        var settings = GetCurrentAcScheduleSettings();
        return settings.TryGetValue(timeBlock, out setting);
    }

    public TimeBlock? FindCurrentTimeBlock()
    {
        var currentTime = _scheduler.Now.LocalDateTime;
        _logger.LogDebug("Finding time block for current hour: {CurrentHour}", currentTime.Hour);

        foreach (var kv in GetCurrentAcScheduleSettings().OrderBy(kv => kv.Value.HourStart))
        {
            if (
                TimeRange.IsTimeInBetween(
                    currentTime.TimeOfDay,
                    kv.Value.HourStart,
                    kv.Value.HourEnd
                )
            )
            {
                _logger.LogDebug(
                    "Found matching time block: {TimeBlock} (range: {StartHour}-{EndHour})",
                    kv.Key,
                    kv.Value.HourStart,
                    kv.Value.HourEnd
                );
                return kv.Key;
            }
        }

        _logger.LogDebug("No time block found for current hour {CurrentHour}", currentTime.Hour);
        return null;
    }

    private Dictionary<TimeBlock, AcScheduleSetting>? _cachedAcSettings;

    private Dictionary<TimeBlock, AcScheduleSetting> GetCurrentAcScheduleSettings()
    {
        if (_cachedAcSettings != null)
        {
            return _cachedAcSettings;
        }

        _cachedAcSettings = new()
        {
            [TimeBlock.Sunrise] = new(
                NormalTemp: 27,
                PowerSavingTemp: 27,
                CoolTemp: 24,
                PassiveTemp: 27,
                Mode: HaEntityStates.DRY,
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
