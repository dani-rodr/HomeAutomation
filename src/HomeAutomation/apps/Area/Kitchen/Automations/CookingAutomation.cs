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
            .StateChanges()
            .WhenStateIsFor(
                s => s?.State > boilingPowerThreshold,
                TimeSpan.FromMinutes(minutes),
                SchedulerProvider.Current
            )
            .Subscribe(_ =>
            {
                if (entities.AirFryerStatus.IsUnavailable())
                {
                    _inductionTurnOff.Press();
                    Logger.LogDebug(
                        "Auto-turned off induction cooker after {Minutes} minutes of boiling",
                        minutes
                    );
                }
            });
    }

    private IDisposable AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePowerThreshold = 100;
        return entities
            .RiceCookerPower.StateChanges()
            .WhenStateIsFor(
                s => s?.State < riceCookerIdlePowerThreshold,
                TimeSpan.FromMinutes(minutes),
                SchedulerProvider.Current
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
