using System.Collections.Generic;
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
    private readonly Dictionary<string, (int NormalTemp, int PowerSavingTemp, string Mode)> _acScheduleSettings = new()
    {
        ["morning"] = (27, 27, HaEntityStates.DRY),
        ["afternoon"] = (25, 27, HaEntityStates.COOL),
        ["night"] = (22, 25, HaEntityStates.COOL),
    };

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        yield return scheduler.ScheduleCron("0 6 * * *", () => ApplyAcSchedule("morning"));
        yield return scheduler.ScheduleCron("0 18 * * *", () => ApplyAcSchedule("afternoon"));
        yield return scheduler.ScheduleCron("0 22 * * *", () => ApplyAcSchedule("night"));
    }

    private void ApplyAcSchedule(string timeOfDay)
    {
        if (!_acScheduleSettings.TryGetValue(timeOfDay, out var setting))
        {
            return;
        }
        var targetTemp = IsPowerSavingMode() ? setting.PowerSavingTemp : setting.NormalTemp;
        if (timeOfDay != "morning" && ShouldSkipCooling(targetTemp))
        {
            return;
        }
        ApplyAcSettings(targetTemp, setting.Mode);

        if (timeOfDay == "morning")
        {
            ActivateFanIfOccupied();
        }
    }

    private bool IsPowerSavingMode() => powerSavingMode.IsOn();

    private void ApplyAcSettings(int temperature, string hvacMode)
    {
        ac.SetTemperature(temperature);
        ac.SetHvacMode(hvacMode);
        ac.SetFanMode(HaEntityStates.AUTO);
    }

    private bool ShouldSkipCooling(double? threshold) =>
        !ac.IsOn() && !ac.IsCool() && ac.Attributes?.Temperature <= threshold;

    private void ActivateFanIfOccupied()
    {
        if (IsOccupied() && ac.Attributes?.CurrentTemperature >= 24)
        {
            fanSwitch.TurnOn();
        }
    }

    private bool IsOccupied() => motionSensor.State.IsOn();

    private bool IsDoorOpen() => doorSensor.State.IsOn();
}
