namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class MotionAutomation(Entities entities, ILogger logger)
    : MotionAutomationBase(
        entities.Switch.KitchenMotionSensor,
        entities.BinarySensor.KitchenMotionSensors,
        entities.Light.RgbLightStrip,
        logger,
        entities.Number.Ld2410Esp325StillTargetDelay
    )
{
    protected override int SensorWaitTime => 15;
    protected override int SensorActiveDelayValue => 15;
    protected override int SensorInactiveDelayValue => 1;
    private readonly BinarySensorEntity _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.StateChanges().IsOnForSeconds(5).Subscribe(_ => Light.TurnOn()),
            MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [SetupDelayOnPowerPlug()];

    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations() => [SetupMotionSensorReactivation()];

    private IDisposable SetupDelayOnPowerPlug() =>
        _powerPlug.StateChanges().IsOn().Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));

    private IDisposable SetupMotionSensorReactivation() =>
        MotionSensor.StateChanges().IsOffForHours(1).Subscribe(_ => MasterSwitch?.TurnOn());
}
