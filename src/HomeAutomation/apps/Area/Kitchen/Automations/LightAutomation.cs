namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class LightAutomation(IKitchenLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    private readonly BinarySensorEntity _powerPlug = entities.PowerPlug;

    protected override int SensorWaitTime => 20;
    protected override int SensorActiveDelayValue => 20;
    protected override int SensorInactiveDelayValue => 1;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor
                .OnOccupied(new(CheckImmediately: true, IgnoreUnavailableState: true, Seconds: 1))
                .Subscribe(_ => Light.TurnOn()),
            MotionSensor.OnCleared(new(CheckImmediately: true)).Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() =>
        [SetupDelayOnPowerPlug()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [SetupMotionSensorReactivation()];

    private IDisposable SetupDelayOnPowerPlug() =>
        _powerPlug
            .OnOccupied()
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));

    private IDisposable SetupMotionSensorReactivation() =>
        MotionSensor.OnCleared(new(Hours: 1)).Subscribe(_ => MasterSwitch.TurnOn());
}
