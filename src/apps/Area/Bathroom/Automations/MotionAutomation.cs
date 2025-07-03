namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation(
    IMotionAutomationEntities entities,
    IDimmingLightController dimmingController,
    ILogger<MotionAutomation> logger
) : MotionAutomationBase(entities, logger)
{
    public override void StartAutomation()
    {
        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);

        base.StartAutomation();
    }

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
