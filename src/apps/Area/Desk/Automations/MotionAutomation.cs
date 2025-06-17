namespace HomeAutomation.apps.Area.Desk.Automations;

/// <summary>
/// Desk area motion automation that controls lighting based on presence detection
/// Handles desk-specific presence patterns and integrates with display/lighting systems
/// </summary>
public class MotionAutomation(
    IMotionAutomationEntities entities,
    IDimmingLightController dimmingController,
    ILogger logger
) : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
{
    public override void StartAutomation()
    {
        dimmingController.SetSensorActiveDelayValue(SensorActiveDelayValue);

        base.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(e => dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .StateChanges()
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
