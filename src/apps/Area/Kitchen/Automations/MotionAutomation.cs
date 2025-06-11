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
    private readonly BinarySensorEntity _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.StateChanges().IsOnForSeconds(5).Subscribe(_ => Light.TurnOn()),
            MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() =>
        [
            .. base.GetSensorDelayAutomations(),
            _powerPlug.StateChanges().IsOn().Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue)),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations() => [SetupMotionSensorReactivation()];

    private IDisposable SetupMotionSensorReactivation() =>
        MotionSensor.StateChanges().IsOffForHours(1).Subscribe(_ => MasterSwitch?.TurnOn());
}
