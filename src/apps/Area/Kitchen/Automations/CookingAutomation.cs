using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(ICookingAutomationEntities entities, ILogger logger) : AutomationBase(logger)
{
    private readonly NumericSensorEntity _riceCookerPower = entities.RiceCookerPower;
    private readonly SwitchEntity _riceCookerSwitch = entities.RiceCookerSwitch;
    private readonly SensorEntity _airFryerStatus = entities.AirFryerStatus;
    private readonly ButtonEntity _inductionTurnOff = entities.InductionTurnOff;
    private readonly NumericSensorEntity _inductionPower = entities.InductionPower;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return AutoTurnOffRiceCookerOnIdle(minutes: 10);
        yield return AutoTurnOffAfterBoilingWater(minutes: 12);
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable AutoTurnOffAfterBoilingWater(int minutes)
    {
        var boilingPowerThreshold = 1550;
        return _inductionPower
            .StateChanges()
            .WhenStateIsForMinutes(s => s?.State > boilingPowerThreshold, minutes)
            .Subscribe(_ =>
            {
                if (_airFryerStatus.State == HaEntityStates.UNAVAILABLE)
                {
                    _inductionTurnOff.Press();
                    Logger.LogInformation(
                        "Auto-turned off induction cooker after {Minutes} minutes of boiling",
                        minutes
                    );
                }
            });
    }

    private IDisposable AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePowerThreshold = 100;
        return _riceCookerPower
            .StateChanges()
            .WhenStateIsForMinutes(s => s?.State < riceCookerIdlePowerThreshold, minutes)
            .Subscribe(_ =>
            {
                _riceCookerSwitch.TurnOff();
                Logger.LogInformation("Auto-turned off rice cooker after {Minutes} minutes idle", minutes);
            });
    }
}
