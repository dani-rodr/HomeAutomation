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
    private readonly InputBooleanEntity _powerSavingMode = entities.InputBoolean.AcPowerSavingMode;
    private readonly SwitchEntity _fanSwitch = entities.Switch.Sonoff100238104e1;
    private readonly Dictionary<TimeBlock, AcScheduleSetting> _acScheduleSettings = new()
    {
        [TimeBlock.Morning] = new(27, 27, 25, HaEntityStates.DRY, true, HourStart: 6, HourEnd: 18),
        [TimeBlock.Afternoon] = new(25, 27, 22, HaEntityStates.COOL, false, HourStart: 18, HourEnd: 22),
        [TimeBlock.Night] = new(22, 25, 20, HaEntityStates.COOL, false, HourStart: 22, HourEnd: 6),
    };

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        foreach (var automation in GetScheduledAutomations())
        {
            yield return automation;
        }
        yield return _doorSensor.StateChanges().IsOff().Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
        yield return _doorSensor
            .StateChanges()
            .WhenStateIsForMinutes(HaEntityStates.ON, 5)
            .Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
        yield return _motionSensor
            .StateChanges()
            .WhenStateIsForMinutes(HaEntityStates.OFF, 10)
            .Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
    }

    private IEnumerable<IDisposable> GetScheduledAutomations()
    {
        foreach (var (timeBlock, setting) in _acScheduleSettings)
        {
            var hour = setting.HourStart;
            string cron = $"0 {hour} * * *";
            yield return _scheduler.ScheduleCron(cron, () => ApplyAcSettings(timeBlock));
        }
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
        ActivateFanIfOccupied(setting.ActivateFan, targetTemp);
    }

    private int GetTemperature(AcScheduleSetting setting) =>
        _powerSavingMode.IsOn() ? setting.PowerSavingTemp
        : _doorSensor.IsClosed() ? setting.ClosedDoorTemp
        : setting.NormalTemp;

    private void ApplyAcSettings(int temperature, string hvacMode)
    {
        _ac.SetTemperature(temperature);
        _ac.SetHvacMode(hvacMode);
        _ac.SetFanMode(HaEntityStates.AUTO);
    }

    private void ActivateFanIfOccupied(bool activateFan, int targetTemp)
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
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
);
