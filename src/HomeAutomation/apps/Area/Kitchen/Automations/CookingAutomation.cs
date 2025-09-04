namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(ICookingEntities entities, ILogger<CookingAutomation> logger)
    : ToggleableAutomation(entities.MasterSwitch, logger)
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
            .OnChanges(new(Minutes: minutes, Condition: s => s?.State > boilingPowerThreshold))
            .Where(_ => entities.AirFryerStatus.IsUnavailable())
            .Subscribe(_ =>
            {
                _inductionTurnOff.Press();
                Logger.LogDebug(
                    "Auto-turned off induction cooker after {Minutes} minutes of boiling",
                    minutes
                );
            });
    }

    private IDisposable AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePowerThreshold = 100;
        return entities
            .RiceCookerPower.OnChanges(
                new(Minutes: minutes, Condition: s => s?.State < riceCookerIdlePowerThreshold)
            )
            .Subscribe(_ =>
            {
                entities.RiceCookerSwitch.TurnOff();
                Logger.LogDebug(
                    "Auto-turned off rice cooker after {Minutes} minutes idle",
                    minutes
                );
            });
    }
}
