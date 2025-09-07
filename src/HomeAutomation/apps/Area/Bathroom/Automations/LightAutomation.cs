namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class LightAutomation(
    ILightAutomationEntities entities,
    IDimmingLightController dimmingController,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private bool masterSwitchEnabledByPantry = true;

    public override void StartAutomation()
    {
        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);

        base.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [
            MotionSensor
                .OnOccupied(new(Seconds: 2))
                .Where(_ => masterSwitchEnabledByPantry)
                .Subscribe(_ => MasterSwitch.TurnOn()),
            MasterSwitch.OnTurnedOn().Subscribe(_ => masterSwitchEnabledByPantry = true),
            MasterSwitch
                .OnTurnedOff(new(Minutes: 5))
                .Subscribe(_ => masterSwitchEnabledByPantry = false),
        ];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor
            .OnOccupied()
            .Subscribe(e => dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .OnCleared()
            .Subscribe(async _ => await dimmingController.OnMotionStoppedAsync(Light));
    }

    public override void Dispose()
    {
        dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
