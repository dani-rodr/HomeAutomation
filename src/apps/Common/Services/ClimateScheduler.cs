using System.Diagnostics.CodeAnalysis;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class ClimateScheduler : IClimateScheduler
{
    private readonly IClimateWeatherEntities _entities;
    private readonly IScheduler _scheduler;
    private readonly ILogger _logger;

    public ClimateScheduler(IClimateWeatherEntities entities, IScheduler scheduler, ILogger logger)
    {
        _entities = entities;
        _scheduler = scheduler;
        _logger = logger;
        AcScheduleSetting.SetWeatherSensor(entities.Weather);
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
        // Lazily initialize cache if needed
        var settings = GetCurrentAcScheduleSettings();
        return settings.TryGetValue(timeBlock, out setting);
    }

    public TimeBlock? FindCurrentTimeBlock()
    {
        var currentHour = _scheduler.Now.Hour;
        _logger.LogDebug("Finding time block for current hour: {CurrentHour}", currentHour);

        foreach (var kv in GetCurrentAcScheduleSettings())
        {
            if (
                TimeRange.IsTimeInBetween(
                    _scheduler.Now.TimeOfDay,
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

        _logger.LogDebug("No time block found for current hour {CurrentHour}", currentHour);
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

    public void LogCurrentAcScheduleSettings()
    {
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

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

public class AcScheduleSetting(
    int NormalTemp,
    int PowerSavingTemp,
    int CoolTemp,
    int PassiveTemp,
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
)
{
    public int NormalTemp { get; } = NormalTemp;
    public int PowerSavingTemp { get; } = PowerSavingTemp;
    public int CoolTemp { get; } = CoolTemp;
    public int PassiveTemp { get; } = PassiveTemp;
    public string Mode { get; } = Mode;
    public bool ActivateFan { get; } = ActivateFan;
    public int HourStart { get; } = HourStart;
    public int HourEnd { get; } = HourEnd;
    private static WeatherEntity? _weather;

    public static void SetWeatherSensor(WeatherEntity weather) => _weather = weather;

    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;

    public int GetTemperature(bool occupied, bool doorOpen, bool powerSaving)
    {
        bool isCold = _weather != null && !_weather.IsSunny();
        return (occupied, doorOpen, powerSaving, isCold) switch
        {
            (_, _, true, _) => PowerSavingTemp,
            (true, false, _, _) => CoolTemp,
            (_, true, _, true) => NormalTemp,
            (true, true, _, false) => NormalTemp,
            (false, true, _, false) => PassiveTemp,
            (false, false, _, _) => PassiveTemp,
        };
    }
}
