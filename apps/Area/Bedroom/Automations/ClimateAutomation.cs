using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(Entities entities, IScheduler scheduler, ILogger<Bedroom> logger)
    : AutomationBase(logger, entities.Switch.AcAutomation)
{
    private readonly IScheduler scheduler = scheduler;
    private readonly ClimateEntity ac = entities.Climate.Ac;
    private readonly BinarySensorEntity motionSensor = entities.BinarySensor.BedroomPresenceSensors;
    private readonly BinarySensorEntity doorSensor = entities.BinarySensor.ContactSensorDoor;
    private readonly InputBooleanEntity powerSavingMode = entities.InputBoolean.AcPowerSavingMode;
    private readonly SwitchEntity fanSwitch = entities.Switch.Sonoff100238104e1;
    private readonly Dictionary<TimeBlock, AcScheduleSetting> _acScheduleSettings = new()
    {
        [TimeBlock.Morning] = new(27, 27, 25, HaEntityStates.DRY, true, 6, 18),
        [TimeBlock.Afternoon] = new(25, 27, 22, HaEntityStates.COOL, false, 18, 22),
        [TimeBlock.Night] = new(22, 25, 20, HaEntityStates.COOL, false, 22, 6),
    };

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        yield return scheduler.ScheduleCron("0 6 * * *", () => ApplyAcSettings(TimeBlock.Morning));
        yield return scheduler.ScheduleCron("0 18 * * *", () => ApplyAcSettings(TimeBlock.Afternoon));
        yield return scheduler.ScheduleCron("0 22 * * *", () => ApplyAcSettings(TimeBlock.Night));
        yield return doorSensor.StateChanges().IsOff().Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
        yield return doorSensor
            .StateChanges()
            .WhenStateIsForMinutes(HaEntityStates.ON, 5)
            .Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
        yield return motionSensor
            .StateChanges()
            .WhenStateIsForMinutes(HaEntityStates.OFF, 10)
            .Subscribe(_ => ApplyAcSettings(FindTimeBlock()));
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
        if (timeBlock is null || !_acScheduleSettings.TryGetValue(timeBlock.Value, out var setting) || !ac.IsOn())
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
        ActivateFanIfOccupied(setting.ActivateFan);
    }

    private int GetTemperature(AcScheduleSetting setting) =>
        powerSavingMode.IsOn() ? setting.PowerSavingTemp
        : IsDoorClosed() ? setting.ClosedDoorTemp
        : setting.NormalTemp;

    private void ApplyAcSettings(int temperature, string hvacMode)
    {
        ac.SetTemperature(temperature);
        ac.SetHvacMode(hvacMode);
        ac.SetFanMode(HaEntityStates.AUTO);
    }

    private void ActivateFanIfOccupied(bool activateFan)
    {
        var isHot = ac.Attributes?.CurrentTemperature >= 24;
        if (activateFan && IsOccupied() && isHot)
        {
            fanSwitch.TurnOn();
        }
    }

    private bool IsOccupied() => motionSensor.State.IsOn();

    private bool IsDoorClosed() => doorSensor.State.IsOff();
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
