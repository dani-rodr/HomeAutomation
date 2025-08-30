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
                .StateChanges()
                .IsOn()
                .ForSeconds(2)
                .Subscribe(_ =>
                {
                    if (masterSwitchEnabledByPantry)
                    {
                        MasterSwitch.TurnOn();
                    }
                }),
            MasterSwitch.StateChanges().IsOn().Subscribe(_ => masterSwitchEnabledByPantry = true),
            MasterSwitch
                .StateChanges()
                .IsOff()
                .ForMinutes(5)
                .Subscribe(_ => masterSwitchEnabledByPantry = false),
        ];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor
            .StateChangesWithCurrent()
            .IsOn()
            .Subscribe(e => dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .StateChangesWithCurrent()
            .IsOff()
            .Subscribe(async _ => await dimmingController.OnMotionStoppedAsync(Light));
    }

    public override void Dispose()
    {
        dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
