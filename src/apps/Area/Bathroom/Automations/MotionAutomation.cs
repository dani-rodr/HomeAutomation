namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation : MotionAutomationBase
{
    private readonly DimmingLightController _dimmingController;

    public MotionAutomation(IMotionAutomationEntities entities, ILogger logger)
        : base(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
    {
        _dimmingController = new DimmingLightController(SensorActiveDelayValue, entities.SensorDelay);
    }

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(e => _dimmingController.OnMotionDetected(Light));
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await _dimmingController.OnMotionStoppedAsync(Light));
    }

    public override void Dispose()
    {
        _dimmingController?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
