namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation : MotionAutomationBase
{
    private readonly DimmingLightController _dimmingController;

    public MotionAutomation(Entities entities, ILogger logger)
        : base(
            entities.Switch.BathroomMotionSensor,
            entities.BinarySensor.BathroomPresenceSensors,
            entities.Light.BathroomLights,
            logger,
            entities.Number.ZEsp32C62StillTargetDelay
        )
    {
        _dimmingController = new DimmingLightController(
            sensorActiveDelayValue: SensorActiveDelayValue,
            sensorDelay: entities.Number.ZEsp32C62StillTargetDelay
        );
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
