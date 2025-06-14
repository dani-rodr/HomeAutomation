using System.Linq;
using System.Text.Json;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(IClimateAutomationEntities entities, IScheduler scheduler, ILogger logger)
    : AutomationBase(logger, entities.MasterSwitch)
{
    private readonly IScheduler _scheduler = scheduler;
    private readonly ClimateEntity _ac = entities.AirConditioner;
    private readonly BinarySensorEntity _motionSensor = entities.MotionSensor;
    private readonly BinarySensorEntity _doorSensor = entities.DoorSensor;
    private readonly SwitchEntity _fanSwitch = entities.FanSwitch;
    private readonly InputBooleanEntity _isPowerSavingMode = entities.PowerSavingMode;
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
                NormalTemp: 25,
                PowerSavingTemp: 27,
                CoolTemp: 24,
                PassiveTemp: 27,
                Mode: HaEntityStates.DRY,
                ActivateFan: true,
                HourStart: entities.SunRising.LocalHour(),
                HourEnd: entities.SunSetting.LocalHour()
            ),

            [TimeBlock.Sunset] = new(
                NormalTemp: 24,
                PowerSavingTemp: 27,
                CoolTemp: 22,
                PassiveTemp: 25,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: entities.SunSetting.LocalHour(),
                HourEnd: entities.SunMidnight.LocalHour()
            ),

            [TimeBlock.Midnight] = new(
                NormalTemp: 24,
                PowerSavingTemp: 25,
                CoolTemp: 20,
                PassiveTemp: 25,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: entities.SunMidnight.LocalHour(),
                HourEnd: entities.SunRising.LocalHour()
            ),
        };

        return _cachedAcSettings;
    }

    public override void StartAutomation()
    {
        base.StartAutomation();
        Logger.LogDebug(
            "AC schedule settings initialized based on current sun sensor values. HourStart and HourEnd may vary daily depending on sunrise, sunset, and midnight times."
        );
        LogCurrentAcScheduleSettings();
    }

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return _scheduler.ScheduleCron(
            "0 0 * * *",
            () =>
            {
                Logger.LogInformation("Midnight AC schedule refresh triggered");
                InvalidateAcSettingsCache();
            }
        );
        yield return _ac.StateAllChanges().IsManuallyOperated().Subscribe(TurnOffMasterSwitchOnManualOperation);
        yield return _motionSensor
            .StateChangesWithCurrent()
            .IsOffForHours(1)
            .Where(_ => MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
    }

    private void TurnOffMasterSwitchOnManualOperation(StateChange e)
    {
        var (oldTemp, newTemp) = e.GetAttributeChange<double?>("temperature");

        Logger.LogInformation(
            "AC state changed: {OldState} ➜ {NewState} | Temp: {OldTemp} ➜ {NewTemp} | By: {User}",
            e.Old?.State,
            e.New?.State,
            oldTemp?.ToString() ?? "N/A",
            newTemp?.ToString() ?? "N/A",
            e.UserId() ?? "unknown"
        );

        if (oldTemp.HasValue && newTemp.HasValue && oldTemp != newTemp)
        {
            MasterSwitch?.TurnOff();
        }
    }

    private void LogCurrentAcScheduleSettings()
    {
        foreach (var kvp in GetCurrentAcScheduleSettings())
        {
            var setting = kvp.Value;
            Logger.LogDebug(
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

    private void InvalidateAcSettingsCache()
    {
        _cachedAcSettings = null;
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            .. GetScheduledAutomations(),
            .. GetSensorBasedAutomations(),
            .. GetHousePresenceAutomations(),
            .. GetFanModeToggleAutomation(),
        ];

    private IEnumerable<IDisposable> GetScheduledAutomations()
    {
        foreach (var (timeBlock, setting) in GetCurrentAcScheduleSettings())
        {
            var hour = setting.HourStart;
            if (hour < 0 || hour > 23)
            {
                Logger.LogWarning(
                    "Invalid HourStart {HourStart} for TimeBlock {TimeBlock}. Skipping schedule.",
                    hour,
                    timeBlock
                );
                continue;
            }
            string cron = $"0 {hour} * * *";
            yield return _scheduler.ScheduleCron(cron, () => ApplyScheduledAcSettings(timeBlock));
        }
    }

    private IEnumerable<IDisposable> GetSensorBasedAutomations()
    {
        yield return _doorSensor.StateChanges().IsClosed().Subscribe(ApplyTimeBasedAcSetting);
        yield return _doorSensor.StateChanges().IsOnForMinutes(5).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.StateChanges().IsOffForMinutes(10).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.StateChanges().IsOn().Subscribe(ApplyTimeBasedAcSetting);
    }

    private void ApplyTimeBasedAcSetting(StateChange e)
    {
        Logger.LogDebug(
            "ApplyTimeBasedAcSetting triggered by sensor: {EntityId}, NewState: {State}",
            e.New?.EntityId,
            e.New?.State
        );

        ApplyScheduledAcSettings(FindTimeBlock());
    }

    private IEnumerable<IDisposable> GetHousePresenceAutomations()
    {
        var houseEmpty = entities.HouseSensor;
        yield return houseEmpty.StateChanges().IsOffForHours(1).Subscribe(_ => _ac.TurnOff());
        yield return houseEmpty
            .StateChanges()
            .IsOn()
            .Subscribe(e =>
            {
                var last = e.Old?.LastChanged;
                var current = e.New?.LastChanged;

                if (!(last.HasValue && current.HasValue))
                {
                    return;
                }
                var timeThresholdMinutes = 20;

                var durationEmptyMinutes = (current.Value - last.Value).TotalMinutes;
                if (durationEmptyMinutes < timeThresholdMinutes)
                {
                    Logger.LogInformation(
                        "House was only empty for {Minutes} minutes. Skipping AC change.",
                        durationEmptyMinutes
                    );
                    return;
                }
                Logger.LogInformation("House was empty for {Minutes} minutes", durationEmptyMinutes);
                _ac.TurnOn();
                ApplyTimeBasedAcSetting(e);
            });
    }

    private IEnumerable<IDisposable> GetFanModeToggleAutomation()
    {
        yield return entities
            .AcFanModeToggle.StateChanges()
            .Where(e => e.IsValidButtonPress())
            .Subscribe(_ =>
            {
                var modes = new[]
                {
                    HaEntityStates.AUTO,
                    HaEntityStates.LOW,
                    HaEntityStates.MEDIUM,
                    HaEntityStates.HIGH,
                };
                var current = _ac.Attributes?.FanMode;
                var index = Array.IndexOf(modes, current);
                var next = modes[(index + 1) % modes.Length];
                _ac.SetFanMode(next);
            });
    }

    private TimeBlock? FindTimeBlock()
    {
        foreach (var kv in GetCurrentAcScheduleSettings())
        {
            if (TimeRange.IsCurrentTimeInBetween(kv.Value.HourStart, kv.Value.HourEnd))
            {
                return kv.Key;
            }
        }
        return null;
    }

    private void ApplyScheduledAcSettings(TimeBlock? timeBlock)
    {
        if (
            timeBlock is null
            || !GetCurrentAcScheduleSettings().TryGetValue(timeBlock.Value, out var setting)
            || !_ac.IsOn()
        )
        {
            return;
        }

        int targetTemp = GetTemperature(setting);
        if (
            _ac.Attributes?.Temperature <= targetTemp
            && string.Equals(_ac.State, setting.Mode, StringComparison.OrdinalIgnoreCase)
        )
        {
            Logger.LogDebug(
                "Skipping ApplyAcSettings: AC already set less than or equal TargetTemp {Temp} and Mode {Mode}",
                targetTemp,
                setting.Mode
            );
            return;
        }
        Logger.LogDebug(
            "ApplyAcSchedule: Applying schedule for {TimeBlock} with target temp {TargetTemp} and mode {Mode}.",
            timeBlock.Value,
            targetTemp,
            setting.Mode
        );
        SetAcTemperatureAndMode(targetTemp, setting.Mode);
        ConditionallyActivateFan(setting.ActivateFan, targetTemp);
    }

    private int GetTemperature(AcScheduleSetting setting)
    {
        bool isOccupied = _motionSensor.IsOccupied();
        bool isDoorOpen = _doorSensor.IsOpen();
        bool isPowerSaving = _isPowerSavingMode.IsOn();
        var weather = entities.Weather;
        bool isColdWeather = !weather.IsSunny();

        Logger.LogInformation(
            "Occupied: {Occupied}, DoorOpen: {DoorOpen}, PowerSaving: {PowerSaving} Weather: {WeatherCondition}",
            isOccupied,
            isDoorOpen,
            isPowerSaving,
            weather?.State
        );
        return (isOccupied, isDoorOpen, isPowerSaving, isColdWeather) switch
        {
            (_, _, true, _) => setting.PowerSavingTemp,
            (true, false, _, _) => setting.CoolTemp,
            (_, true, _, true) => setting.NormalTemp,
            (true, true, _, false) => setting.NormalTemp,
            (false, true, _, false) => setting.PassiveTemp,
            (false, false, _, _) => setting.PassiveTemp,
        };
    }

    private void SetAcTemperatureAndMode(int temperature, string hvacMode)
    {
        _ac.SetTemperature(temperature);
        _ac.SetHvacMode(hvacMode);
        _ac.SetFanMode(HaEntityStates.AUTO);
    }

    private void ConditionallyActivateFan(bool activateFan, int targetTemp)
    {
        var isHot = _ac.Attributes?.CurrentTemperature >= targetTemp;
        if (activateFan && _motionSensor.IsOccupied() && isHot)
        {
            _fanSwitch.TurnOn();
        }
    }
}

internal enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

internal record AcScheduleSetting(
    int NormalTemp,
    int PowerSavingTemp,
    int CoolTemp,
    int PassiveTemp,
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
);
