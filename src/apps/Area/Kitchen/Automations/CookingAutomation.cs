namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(ICookingEntities entities, ILogger logger) : AutomationBase(logger)
{
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
                if (entities.AirFryerStatus.IsUnavailable())
                {
                    _inductionTurnOff.Press();
                    Logger.LogDebug("Auto-turned off induction cooker after {Minutes} minutes of boiling", minutes);
                }
            });
    }

    private IDisposable AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePowerThreshold = 100;
        return entities
            .RiceCookerPower.StateChanges()
            .WhenStateIsForMinutes(s => s?.State < riceCookerIdlePowerThreshold, minutes)
            .Subscribe(_ =>
            {
                entities.RiceCookerSwitch.TurnOff();
                Logger.LogDebug("Auto-turned off rice cooker after {Minutes} minutes idle", minutes);
            });
    }
}
