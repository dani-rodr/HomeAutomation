using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Config;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(
    ICookingEntities entities,
    KitchenCookingSettings cookingSettings,
    ILogger<CookingAutomation> logger
) : ToggleableAutomation(entities.MasterSwitch, logger)
{
    private readonly ButtonEntity _inductionTurnOff = entities.InductionTurnOff;

    private readonly NumericSensorEntity _inductionPower = entities.InductionPower;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [AutoTurnOffAfterBoilingWater()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable AutoTurnOffAfterBoilingWater()
    {
        var boilingPowerThreshold = cookingSettings.BoilingPowerThresholdWatts;

        return _inductionPower
            .OnChanges(
                new(
                    Minutes: cookingSettings.BoilingAutoOffMinutes,
                    Condition: s => s?.State > boilingPowerThreshold
                )
            )
            .Where(_ => entities.AirFryerStatus.IsUnavailable())
            .Subscribe(_ =>
            {
                _inductionTurnOff.Press();

                Logger.LogDebug(
                    "Auto-turned off induction cooker after {Minutes} minutes of boiling",
                    cookingSettings.BoilingAutoOffMinutes
                );
            });
    }
}
