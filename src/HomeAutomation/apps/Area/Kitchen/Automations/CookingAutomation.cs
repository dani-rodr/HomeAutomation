namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(ICookingEntities entities, ILogger<CookingAutomation> logger)
    : ToggleableAutomation(entities.MasterSwitch, logger)
{
    private readonly ButtonEntity _inductionTurnOff = entities.InductionTurnOff;
    private readonly NumericSensorEntity _inductionPower = entities.InductionPower;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [AutoTurnOffAfterBoilingWater(minutes: 12)];

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
}
