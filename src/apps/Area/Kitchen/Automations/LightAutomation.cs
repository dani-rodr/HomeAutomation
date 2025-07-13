namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class LightAutomation(IKitchenLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    private readonly BinarySensorEntity _powerPlug = entities.PowerPlug;

    protected override int SensorWaitTime => 30;
    protected override int SensorActiveDelayValue => 60;
    protected override int SensorInactiveDelayValue => 1;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.StateChangesWithCurrent().IsOnForSeconds(2).Subscribe(_ => Light.TurnOn()),
            MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() =>
        [SetupDelayOnPowerPlug()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [SetupMotionSensorReactivation()];

    private IDisposable SetupDelayOnPowerPlug() =>
        _powerPlug
            .StateChanges()
            .IsOn()
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));

    private IDisposable SetupMotionSensorReactivation() =>
        MotionSensor.StateChanges().IsOffForHours(1).Subscribe(_ => MasterSwitch?.TurnOn());
}
