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
    private readonly Dictionary<TimeBlock, AcScheduleSetting> _acScheduleSettings = new()
    {
        [TimeBlock.Morning] = new(
            NormalTemp: 25,
            PowerSavingTemp: 27,
            ClosedDoorTemp: 24,
            UnoccupiedTemp: 27,
            Mode: HaEntityStates.DRY,
            ActivateFan: true,
            HourStart: 6,
            HourEnd: 18
        ),

        [TimeBlock.Afternoon] = new(
            NormalTemp: 24,
            PowerSavingTemp: 27,
            ClosedDoorTemp: 22,
            UnoccupiedTemp: 25,
            Mode: HaEntityStates.COOL,
            ActivateFan: false,
            HourStart: 18,
            HourEnd: 22
        ),

        [TimeBlock.Night] = new(
            NormalTemp: 22,
            PowerSavingTemp: 25,
            ClosedDoorTemp: 20,
            UnoccupiedTemp: 25,
            Mode: HaEntityStates.COOL,
            ActivateFan: false,
            HourStart: 22,
            HourEnd: 6
        ),
    };
    private bool _isHouseEmpty = false;

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
        [
            .. GetScheduledAutomations(),
            .. GetSensorBasedAutomations(),
            .. GetHousePresenceAutomations(),
            .. GetFanModeToggleAutomation(),
        ];

    private IEnumerable<IDisposable> GetScheduledAutomations()
    {
        foreach (var (timeBlock, setting) in _acScheduleSettings)
        {
            var hour = setting.HourStart;
            string cron = $"0 {hour} * * *";
            yield return _scheduler.ScheduleCron(cron, () => ApplyAcSettings(timeBlock));
        }
    }

    private IEnumerable<IDisposable> GetSensorBasedAutomations()
    {
        yield return _doorSensor.StateChanges().IsOff().Subscribe(ApplyTimeBasedAcSetting);
        yield return _doorSensor.StateChanges().IsOnForMinutes(5).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.StateChanges().IsOffForMinutes(10).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.StateChanges().IsOn().Subscribe(ApplyTimeBasedAcSetting);
    }

    private void ApplyTimeBasedAcSetting(StateChange e) => ApplyAcSettings(FindTimeBlock());

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
        foreach (var kv in _acScheduleSettings)
        {
            if (TimeRange.IsCurrentTimeInBetween(kv.Value.HourStart, kv.Value.HourEnd))
                return kv.Key;
        }
        return null;
    }

    private void ApplyAcSettings(TimeBlock? timeBlock)
    {
        if (timeBlock is null || !_acScheduleSettings.TryGetValue(timeBlock.Value, out var setting) || !_ac.IsOn())
        {
            return;
        }

        int targetTemp = GetTemperature(setting);
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
    Morning,
    Afternoon,
    Night,
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
