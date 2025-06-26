using System.Linq;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(IClimateEntities entities, IScheduler scheduler, ILogger logger)
    : AutomationBase(logger, entities.MasterSwitch)
{
    private readonly IScheduler _scheduler = scheduler;
    private readonly ClimateEntity _ac = entities.AirConditioner;
    private readonly BinarySensorEntity _motionSensor = entities.MotionSensor;
    private readonly BinarySensorEntity _doorSensor = entities.Door;
    private readonly SwitchEntity _fanAutomation = entities.FanAutomation;
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
                NormalTemp: 27,
                PowerSavingTemp: 27,
                CoolTemp: 24,
                PassiveTemp: 27,
                Mode: HaEntityStates.DRY,
                ActivateFan: true,
                HourStart: entities.SunRising.LocalHour(),
                HourEnd: entities.SunSetting.LocalHour()
            ),

            [TimeBlock.Sunset] = new(
                NormalTemp: 25,
                PowerSavingTemp: 27,
                CoolTemp: 23,
                PassiveTemp: 27,
                Mode: HaEntityStates.COOL,
                ActivateFan: false,
                HourStart: entities.SunSetting.LocalHour(),
                HourEnd: entities.SunMidnight.LocalHour()
            ),

            [TimeBlock.Midnight] = new(
                NormalTemp: 24,
                PowerSavingTemp: 25,
                CoolTemp: 22,
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
                Logger.LogDebug("Midnight AC schedule refresh triggered");
                InvalidateAcSettingsCache();
            }
        );
        yield return _ac.StateAllChanges()
            .IsManuallyOperated()
            .Subscribe(TurnOffMasterSwitchOnManualOperation);
        yield return _motionSensor
            .StateChangesWithCurrent()
            .IsOffForHours(1)
            .Where(_ => MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
        yield return _doorSensor
            .StateChanges()
            .IsClosed()
            .Subscribe(e =>
            {
                ApplyTimeBasedAcSetting(e);
                MasterSwitch?.TurnOn();
            });
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            .. GetScheduledAutomations(),
            .. GetSensorBasedAutomations(),
            .. GetHousePresenceAutomations(),
            .. GetFanModeToggleAutomation(),
        ];

    private void TurnOffMasterSwitchOnManualOperation(StateChange e)
    {
        var (oldTemp, newTemp) = e.GetAttributeChange<double?>("temperature");

        if (oldTemp.HasValue && newTemp.HasValue && oldTemp != newTemp)
        {
            MasterSwitch?.TurnOff();
            Logger.LogDebug(
                "AC state changed: {OldState} ➜ {NewState} | Temp: {OldTemp} ➜ {NewTemp} | By: {User}",
                e.Old?.State,
                e.New?.State,
                oldTemp?.ToString() ?? "N/A",
                newTemp?.ToString() ?? "N/A",
                e.UserId() ?? "unknown"
            );
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
        yield return _doorSensor
            .StateChanges()
            .IsOnForMinutes(5)
            .Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor
            .StateChanges()
            .IsOffForMinutes(10)
            .Subscribe(ApplyTimeBasedAcSetting);
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
        var houseOccupancy = entities.HouseMotionSensor;
        yield return houseOccupancy.StateChanges().IsOffForHours(1).Subscribe(_ => _ac.TurnOff());
        yield return houseOccupancy
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
                    Logger.LogDebug(
                        "House was only empty for {Minutes} minutes. Skipping AC change.",
                        durationEmptyMinutes
                    );
                    return;
                }
                Logger.LogDebug("House was empty for {Minutes} minutes", durationEmptyMinutes);
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
        var currentHour = DateTime.Now.Hour;
        Logger.LogDebug("Finding time block for current hour: {CurrentHour}", currentHour);

        foreach (var kv in GetCurrentAcScheduleSettings())
        {
            if (TimeRange.IsCurrentTimeInBetween(kv.Value.HourStart, kv.Value.HourEnd))
            {
                Logger.LogDebug(
                    "Found matching time block: {TimeBlock} (range: {StartHour}-{EndHour})",
                    kv.Key,
                    kv.Value.HourStart,
                    kv.Value.HourEnd
                );
                return kv.Key;
            }
        }

        Logger.LogDebug("No time block found for current hour {CurrentHour}", currentHour);
        return null;
    }

    private void ApplyScheduledAcSettings(TimeBlock? timeBlock)
    {
        Logger.LogDebug(
            "AC settings evaluation: TimeBlock={TimeBlock}, AC.IsOn={AcOn}",
            timeBlock?.ToString() ?? "None",
            _ac.IsOn()
        );

        if (timeBlock is null)
        {
            Logger.LogDebug("Skipping AC settings: No active time block");
            return;
        }

        if (!GetCurrentAcScheduleSettings().TryGetValue(timeBlock.Value, out var setting))
        {
            Logger.LogDebug(
                "Skipping AC settings: No settings found for time block {TimeBlock}",
                timeBlock.Value
            );
            return;
        }

        if (!_ac.IsOn())
        {
            Logger.LogDebug("Skipping AC settings: AC is currently OFF");
            return;
        }

        int targetTemp = GetTemperature(setting);
        var currentTemp = _ac.Attributes?.Temperature;
        var currentMode = _ac.State;

        if (
            currentTemp == targetTemp
            && string.Equals(currentMode, setting.Mode, StringComparison.OrdinalIgnoreCase)
        )
        {
            Logger.LogDebug(
                "Skipping AC settings: Already configured correctly - Temp: {CurrentTemp}°C = {TargetTemp}°C, Mode: {CurrentMode} = {TargetMode}",
                currentTemp,
                targetTemp,
                currentMode,
                setting.Mode
            );
            return;
        }

        Logger.LogDebug(
            "Applying AC schedule for {TimeBlock}: {CurrentTemp}°C → {TargetTemp}°C, {CurrentMode} → {TargetMode}, ActivateFan={ActivateFan}",
            timeBlock.Value,
            currentTemp,
            targetTemp,
            currentMode,
            setting.Mode,
            setting.ActivateFan
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

        Logger.LogDebug(
            "Temperature decision inputs: Occupied={Occupied}, DoorOpen={DoorOpen}, PowerSaving={PowerSaving}, Weather={WeatherCondition}, IsCold={IsColdWeather}",
            isOccupied,
            isDoorOpen,
            isPowerSaving,
            weather?.State,
            isColdWeather
        );

        var (selectedTemp, tempType) = (isOccupied, isDoorOpen, isPowerSaving, isColdWeather) switch
        {
            (_, _, true, _) => (setting.PowerSavingTemp, "PowerSaving"),
            (true, false, _, _) => (setting.CoolTemp, "Cool"),
            (_, true, _, true) => (setting.NormalTemp, "Normal"),
            (true, true, _, false) => (setting.NormalTemp, "Normal"),
            (false, true, _, false) => (setting.PassiveTemp, "Passive"),
            (false, false, _, _) => (setting.PassiveTemp, "Passive"),
        };

        Logger.LogDebug(
            "Temperature decision: Selected {TempType} temperature {Temperature}°C based on pattern: (occupied:{Occupied}, doorOpen:{DoorOpen}, powerSaving:{PowerSaving}, coldWeather:{ColdWeather})",
            tempType,
            selectedTemp,
            isOccupied,
            isDoorOpen,
            isPowerSaving,
            isColdWeather
        );

        return selectedTemp;
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
        if (activateFan && isHot)
        {
            _fanAutomation.TurnOn();
            return;
        }
        _fanAutomation.TurnOff();
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
