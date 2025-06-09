using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(Entities entities, ILogger logger) : AutomationBase(logger)
{
    private readonly NumericSensorEntity _riceCookerPower = entities.Sensor.RiceCookerPower;
    private readonly SwitchEntity _riceCookerSwitch = entities.Switch.RiceCookerSocket1;
    private readonly SensorEntity _airFryerStatus = entities.Sensor.CareliSg593061393Maf05aStatusP21;
    private readonly ButtonEntity _inductionTurnOff = entities.Button.InductionCookerPower;
    private readonly NumericSensorEntity _inductionPower = entities.Sensor.SmartPlug3SonoffS31Power;

    protected override IEnumerable<IDisposable> GetStartupAutomations()
    {
        yield return AutoTurnOffRiceCookerOnIdle(minutes: 10);
        yield return AutoTurnOffAfterBoilingWater(minutes: 12);
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() => [];

    private IDisposable AutoTurnOffAfterBoilingWater(int minutes)
    {
        var boilingPowerThreshold = 1550;
        return _inductionPower
            .StateChangesWithCurrent()
            .WhenStateIsForMinutes(s => s?.State > boilingPowerThreshold, minutes)
            .Subscribe(_ =>
            {
                if (_airFryerStatus.State == HaEntityStates.UNAVAILABLE)
                {
                    _inductionTurnOff.Press();
                    Logger.LogInformation("Auto-turned off induction cooker after {Minutes} minutes of boiling", minutes);
                }
            });
    }

    private IDisposable AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePowerThreshold = 100;
        return _riceCookerPower
            .StateChangesWithCurrent()
            .WhenStateIsForMinutes(s => s?.State < riceCookerIdlePowerThreshold, minutes)
            .Subscribe(_ =>
            {
                _riceCookerSwitch.TurnOff();
                Logger.LogInformation("Auto-turned off rice cooker after {Minutes} minutes idle", minutes);
            });
    }
}
