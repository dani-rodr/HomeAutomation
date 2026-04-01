using HomeAutomation.apps.Area.Kitchen.Automations.Entities;
using HomeAutomation.apps.Area.Kitchen.Config;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class LightAutomation(
    IKitchenLightEntities entities,
    KitchenLightSettings settings,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private readonly BinarySensorEntity _powerPlug = entities.PowerPlug;
    private readonly KitchenLightSettings _settings = settings;

    protected override int SensorWaitTime => _settings.SensorWaitSeconds;

    protected override int SensorActiveDelayValue => _settings.SensorActiveDelayValue;

    protected override int SensorInactiveDelayValue => _settings.SensorInactiveDelayValue;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor
                .OnOccupied(new(Seconds: _settings.MotionOnDelaySeconds))
                .Subscribe(_ => Light.TurnOn()),
            MotionSensor.OnCleared().Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() =>
        [SetupDelayOnPowerPlug()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() => [];

    private IDisposable SetupDelayOnPowerPlug() =>
        _powerPlug
            .OnOccupied()
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));
}
