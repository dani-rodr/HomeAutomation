namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class LightAutomation(
    ILightAutomationEntities entities,
    IDimmingLightController dimmingController,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    public override void StartAutomation()
    {
        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);

        base.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [MotionSensor.StateChanges().IsOn().ForSeconds(2).Subscribe(_ => MasterSwitch.TurnOn())];

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
