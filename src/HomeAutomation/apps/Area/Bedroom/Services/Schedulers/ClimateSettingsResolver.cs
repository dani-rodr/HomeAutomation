using System.Reactive.Subjects;
using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Common.Settings;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

public sealed class ClimateSettingsResolver : IClimateSettingsResolver, IDisposable
{
    private const string AreaKey = "bedroom";

    private readonly IAreaSettingsStore _areaSettingsStore;
    private readonly InputBooleanEntity _powerSavingMode;
    private readonly IScheduler _scheduler;
    private readonly IAcTemperatureCalculator _temperatureCalculator;
    private readonly ILogger<ClimateSettingsResolver> _logger;
    private readonly Subject<AreaSettingsChangedEvent> _changes = new();
    private readonly IDisposable _settingsChangesSubscription;
    private ClimateSettings _currentSettings;

    public ClimateSettingsResolver(
        Entities.IClimateSchedulerEntities entities,
        IAreaSettingsStore areaSettingsStore,
        IAreaSettingsChangeNotifier areaSettingsChangeNotifier,
        IAcTemperatureCalculator temperatureCalculator,
        ILogger<ClimateSettingsResolver> logger
    )
    {
        _areaSettingsStore = areaSettingsStore;
        _powerSavingMode = entities.PowerSavingMode;
        _scheduler = SchedulerProvider.Current;
        _temperatureCalculator = temperatureCalculator;
        _logger = logger;
        _currentSettings = LoadClimateSettings();

        _settingsChangesSubscription = areaSettingsChangeNotifier
            .Changes.Where(change =>
                string.Equals(change.AreaKey, AreaKey, StringComparison.OrdinalIgnoreCase)
            )
            .Subscribe(HandleSettingsChanged);

        LogCurrentAcScheduleSettings();
    }

    public IObservable<AreaSettingsChangedEvent> Changes => _changes;

    public IEnumerable<IDisposable> GetSchedules(Action action)
    {
        var settings = _currentSettings;
        foreach (var timeBlock in new[] { TimeBlock.Sunrise, TimeBlock.Sunset, TimeBlock.Midnight })
        {
            var setting = settings.GetByTimeBlock(timeBlock);
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
            _currentSettings.Automation.ResetScheduleCron,
            LogCurrentAcScheduleSettings
        );

    public bool TryGetCurrentSetting(out TimeBlock timeBlock, out ClimateSetting setting)
    {
        if (!TryFindCurrentTimeBlock(out timeBlock))
        {
            setting = default!;
            return false;
        }

        setting = _currentSettings.GetByTimeBlock(timeBlock);
        return true;
    }

    public int CalculateTemperature(ClimateSetting settings, bool isOccupied, bool isDoorOpen) =>
        _temperatureCalculator.CalculateTemperature(
            settings,
            isOccupied,
            isDoorOpen,
            _powerSavingMode.IsOn()
        );

    public WeatherPowerSavingSettings GetWeatherPowerSavingSettings() =>
        _currentSettings.WeatherPowerSaving;

    public ClimateAutomationSettings GetAutomationSettings() => _currentSettings.Automation;

    public void Dispose()
    {
        _settingsChangesSubscription.Dispose();
        _changes.Dispose();
    }

    private bool TryFindCurrentTimeBlock(out TimeBlock timeBlock)
    {
        var currentTime = _scheduler.Now.LocalDateTime;
        _logger.LogDebug("Finding time block for current time: {CurrentTime}", currentTime);

        var settings = _currentSettings;

        // Find the first time block that matches current time
        // Let TimeRange.IsTimeInBetween handle all the overnight range logic
        foreach (var block in new[] { TimeBlock.Sunrise, TimeBlock.Sunset, TimeBlock.Midnight })
        {
            var setting = settings.GetByTimeBlock(block);
            if (
                TimeRange.IsTimeInBetween(currentTime.TimeOfDay, setting.HourStart, setting.HourEnd)
            )
            {
                timeBlock = block;
                _logger.LogDebug(
                    "Found matching time block: {TimeBlock} (range: {StartHour}-{EndHour})",
                    block,
                    setting.HourStart,
                    setting.HourEnd
                );
                return true;
            }

            _logger.LogDebug(
                "Time block {TimeBlock} (range: {StartHour}-{EndHour}) does not match current time {CurrentTime}",
                block,
                setting.HourStart,
                setting.HourEnd,
                currentTime.TimeOfDay
            );
        }

        _logger.LogDebug("No time block found for current hour {CurrentHour}", currentTime.Hour);
        timeBlock = default;
        return false;
    }

    private ClimateSettings LoadClimateSettings() =>
        _areaSettingsStore.GetSettings<ClimateSettings>(AreaKey);

    private void HandleSettingsChanged(AreaSettingsChangedEvent changeEvent)
    {
        _currentSettings = LoadClimateSettings();
        _logger.LogInformation(
            "Reloaded bedroom climate settings ({ChangeType}) at {OccurredAtUtc}.",
            changeEvent.ChangeType,
            changeEvent.OccurredAtUtc
        );

        LogCurrentAcScheduleSettings();
        _changes.OnNext(changeEvent);
    }

    private void LogCurrentAcScheduleSettings()
    {
        _logger.LogDebug("AC schedule settings initialized from Bedroom area settings.");
        var settings = _currentSettings;
        foreach (var timeBlock in new[] { TimeBlock.Sunrise, TimeBlock.Sunset, TimeBlock.Midnight })
        {
            var setting = settings.GetByTimeBlock(timeBlock);
            _logger.LogDebug(
                "TimeBlock {TimeBlock}: DoorOpenTemp={DoorOpenTemp},"
                    + " EcoAwayTemp={EcoAwayTemp}, ComfortTemp={ComfortTemp},"
                    + " AwayTemp={AwayTemp}, Mode={Mode}, ActivateFan={ActivateFan},"
                    + " HourStart={HourStart}, HourEnd={HourEnd}",
                timeBlock,
                setting.DoorOpenTemp,
                setting.EcoAwayTemp,
                setting.ComfortTemp,
                setting.AwayTemp,
                setting.Mode,
                setting.ActivateFan,
                setting.HourStart,
                setting.HourEnd
            );
        }
    }
}
