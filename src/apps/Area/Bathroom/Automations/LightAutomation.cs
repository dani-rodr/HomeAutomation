namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class LightAutomation(
    ILightAutomationEntities entities,
    MotionAutomationBase motionAutomation,
    IDimmingLightController dimmingController,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{
    public override void StartAutomation()
    {
        dimmingController.SetSensorActiveDelayValue(MotionAutomation.SensorActiveDelayValue);

        base.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionAutomation
            .GetMotionSensor()
            .StateChangesWithCurrent()
            .IsOn()
            .Subscribe(e => dimmingController.OnMotionDetected(Light));
        yield return MotionAutomation
            .GetMotionSensor()
            .StateChangesWithCurrent()
            .IsOff(ignorePreviousUnavailable: false)
            .Subscribe(async _ => await dimmingController.OnMotionStoppedAsync(Light));
    }

    public override void Dispose()
    {
        dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
