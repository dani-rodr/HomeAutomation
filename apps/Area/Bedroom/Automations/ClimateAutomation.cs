using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(Entities entities, IScheduler scheduler, ILogger<Bedroom> logger)
    : AutomationBase(logger, entities.Switch.AcAutomation)
{
    private readonly IScheduler _scheduler = scheduler;
    private readonly ClimateEntity _ac = entities.Climate.Ac;
    private readonly BinarySensorEntity _motionSensor = entities.BinarySensor.BedroomPresenceSensors;
    private readonly BinarySensorEntity _doorSensor = entities.BinarySensor.ContactSensorDoor;
    private readonly SwitchEntity _fanSwitch = entities.Switch.Sonoff100238104e1;
    private Dictionary<TimeBlock, AcScheduleSetting> GetCurrentAcScheduleSettings() => new()
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
    private bool _isHouseEmpty = false;
    public override void StartAutomation()
    {
        base.StartAutomation();
        Logger.LogDebug("AC schedule settings initialized based on current sun sensor values. HourStart and HourEnd may vary daily depending on sunrise, sunset, and midnight times.");

        foreach (var kvp in GetCurrentAcScheduleSettings())
        {
            var setting = kvp.Value;

            Logger.LogDebug("TimeBlock {TimeBlock}: NormalTemp={NormalTemp}, PowerSavingTemp={PowerSavingTemp}, ClosedDoorTemp={ClosedDoorTemp}, UnoccupiedTemp={UnoccupiedTemp}, Mode={Mode}, ActivateFan={ActivateFan}, HourStart={HourStart}, HourEnd={HourEnd}",
                kvp.Key, setting.NormalTemp, setting.PowerSavingTemp, setting.ClosedDoorTemp, setting.UnoccupiedTemp,
                setting.Mode, setting.ActivateFan, setting.HourStart, setting.HourEnd);
        }
        _scheduler.ScheduleCron("0 0 * * *", RestartAutomations);
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
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
                Logger.LogWarning("Invalid HourStart {HourStart} for TimeBlock {TimeBlock}. Skipping schedule.", hour, timeBlock);
                continue;
            }
            string cron = $"0 {hour} * * *";
            yield return _scheduler.ScheduleCron(cron, () => ApplyAcSettings(timeBlock));
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

        ApplyAcSettings(FindTimeBlock());
    }

    private IEnumerable<IDisposable> GetHousePresenceAutomations()
    {
        var houseEmpty = entities.BinarySensor.House;

        yield return houseEmpty.StateChangesWithCurrent().IsOffForMinutes(20).Subscribe(_ => _isHouseEmpty = true);
        yield return houseEmpty
            .StateChanges()
            .Where(e => e.IsOn() && _isHouseEmpty)
            .Subscribe(e =>
            {
                _isHouseEmpty = false;
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

    private void ApplyAcSettings(TimeBlock? timeBlock)
    {
        if (timeBlock is null || !GetCurrentAcScheduleSettings().TryGetValue(timeBlock.Value, out var setting) || !_ac.IsOn())
        {
            return;
        }

        int targetTemp = GetTemperature(setting);
        if (_ac.Attributes?.Temperature == setting.NormalTemp && string.Equals(_ac.State, setting.Mode, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogDebug("Skipping ApplyAcSettings: AC already set to TargetTemp {Temp} and Mode {Mode}", setting.NormalTemp, setting.Mode);
            return;
        }
        Logger.LogDebug(
            "ApplyAcSchedule: Applying schedule for {TimeBlock} with target temp {TargetTemp} and mode {Mode}.",
            timeBlock.Value,
            targetTemp,
            setting.Mode
        );
        ApplyAcSettings(targetTemp, setting.Mode);
        ActivateFan(setting.ActivateFan, targetTemp);
    }

    private int GetTemperature(AcScheduleSetting setting)
    {
        if (_motionSensor.IsOff())
        {
            return setting.UnoccupiedTemp;
        }

        if (_doorSensor.IsClosed())
        {
            return setting.ClosedDoorTemp;
        }

        if (entities.InputBoolean.AcPowerSavingMode.IsOn())
        {
            return setting.PowerSavingTemp;
        }

        return setting.NormalTemp;
    }

    private void ApplyAcSettings(int temperature, string hvacMode)
    {
        _ac.SetTemperature(temperature);
        _ac.SetHvacMode(hvacMode);
        _ac.SetFanMode(HaEntityStates.AUTO);
    }

    private void ActivateFan(bool activateFan, int targetTemp)
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
