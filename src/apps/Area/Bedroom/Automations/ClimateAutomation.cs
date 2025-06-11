using System.Linq;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(Entities entities, IScheduler scheduler, ILogger logger)
    : AutomationBase(logger, entities.Switch.AcAutomation)
{
    private readonly IScheduler _scheduler = scheduler;
    private readonly ClimateEntity _ac = entities.Climate.Ac;
    private readonly BinarySensorEntity _motionSensor = entities.BinarySensor.BedroomPresenceSensors;
    private readonly BinarySensorEntity _doorSensor = entities.BinarySensor.ContactSensorDoor;
    private readonly SwitchEntity _fanSwitch = entities.Switch.Sonoff100238104e1;
    private Dictionary<TimeBlock, AcScheduleSetting>? _cachedAcSettings;
    private readonly Lock _houseEmptyLock = new();
    private bool _isHouseEmpty = false;

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
                ClosedDoorTemp: 24,
                UnoccupiedTemp: 27,
                Mode: HaEntityStates.DRY,
                ActivateFan: true,
                HourStart: entities.Sensor.SunNextRising.LocalHour(),
                HourEnd: entities.Sensor.SunNextSetting.LocalHour()
            ),

            [TimeBlock.Sunset] = new(
                NormalTemp: 24,
                PowerSavingTemp: 27,
                ClosedDoorTemp: 22,
                UnoccupiedTemp: 25,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: entities.Sensor.SunNextSetting.LocalHour(),
                HourEnd: entities.Sensor.SunNextMidnight.LocalHour()
            ),

            [TimeBlock.Midnight] = new(
                NormalTemp: 22,
                PowerSavingTemp: 25,
                ClosedDoorTemp: 20,
                UnoccupiedTemp: 25,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: entities.Sensor.SunNextMidnight.LocalHour(),
                HourEnd: entities.Sensor.SunNextRising.LocalHour()
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
    }

    private void LogCurrentAcScheduleSettings()
    {
        foreach (var kvp in GetCurrentAcScheduleSettings())
        {
            var setting = kvp.Value;
            Logger.LogDebug(
                "TimeBlock {TimeBlock}: NormalTemp={NormalTemp}, PowerSavingTemp={PowerSavingTemp}, ClosedDoorTemp={ClosedDoorTemp}, UnoccupiedTemp={UnoccupiedTemp}, Mode={Mode}, ActivateFan={ActivateFan}, HourStart={HourStart}, HourEnd={HourEnd}",
                kvp.Key,
                setting.NormalTemp,
                setting.PowerSavingTemp,
                setting.ClosedDoorTemp,
                setting.UnoccupiedTemp,
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
        var houseEmpty = entities.BinarySensor.House;
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
            .Button.AcFanModeToggle.StateChanges()
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
                return kv.Key;
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
            _ac.Attributes?.Temperature == targetTemp
            && string.Equals(_ac.State, setting.Mode, StringComparison.OrdinalIgnoreCase)
        )
        {
            Logger.LogDebug(
                "Skipping ApplyAcSettings: AC already set to TargetTemp {Temp} and Mode {Mode}",
                setting.NormalTemp,
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
        bool isPowerSaving = entities.InputBoolean.AcPowerSavingMode.IsOn();
        var weather = entities.Weather.Home;
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
            (false, true, _, true) => setting.NormalTemp,
            (false, true, _, false) => setting.UnoccupiedTemp,
            (true, false, _, _) => setting.ClosedDoorTemp,
            (true, true, true, _) => setting.PowerSavingTemp,
            _ => setting.NormalTemp,
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
    int ClosedDoorTemp,
    int UnoccupiedTemp,
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
);
