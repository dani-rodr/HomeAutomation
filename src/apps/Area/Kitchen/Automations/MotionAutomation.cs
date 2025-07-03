namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class MotionAutomation(IKitchenMotionEntities entities, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(entities, logger)
{
    private readonly BinarySensorEntity _powerPlug = entities.PowerPlug;

    protected override int SensorWaitTime => 15;
    protected override int SensorActiveDelayValue => 15;
    protected override int SensorInactiveDelayValue => 1;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.StateChangesWithCurrent().IsOnForSeconds(5).Subscribe(_ => Light.TurnOn()),
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
