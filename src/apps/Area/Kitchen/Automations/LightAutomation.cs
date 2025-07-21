namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class LightAutomation(
    IKitchenLightEntities entities,
    MotionAutomationBase motionAutomation,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{
    private readonly BinarySensorEntity _powerPlug = entities.PowerPlug;


    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionAutomation
                .GetMotionSensor()
                .StateChangesWithCurrent()
                .IsOnForSeconds(2)
                .Subscribe(_ => Light.TurnOn()),
            MotionAutomation
                .GetMotionSensor()
                .StateChangesWithCurrent()
                .IsOff()
                .Subscribe(_ => Light.TurnOff()),
        ];

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() =>
        [SetupDelayOnPowerPlug()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [SetupMotionSensorReactivation()];

    private IDisposable SetupDelayOnPowerPlug() =>
        _powerPlug
            .StateChanges()
            .IsOn()
            .Subscribe(_ =>
                MotionAutomation.GetSensorDelay().SetNumericValue(MotionAutomation.SensorActiveDelayValue)
            );

    private IDisposable SetupMotionSensorReactivation() =>
        MotionAutomation
            .GetMotionSensor()
            .StateChanges()
            .IsOffForHours(1)
            .Subscribe(_ => MasterSwitch.TurnOn());
}
