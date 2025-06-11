namespace HomeAutomation.apps.Area.Bedroom;

public class FanAutomation(Entities entities, ILogger logger)
    : FanAutomationBase(
        entities.Switch.BedroomMotionSensor,
        entities.BinarySensor.BedroomPresenceSensors,
        logger,
        entities.Switch.Sonoff100238104e1
    )
{
    protected override bool IsFanManuallyActivated { get; set; } = false;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return Fan.StateChangesWithCurrent()
            .IsManuallyOperated()
            .Subscribe(_ => IsFanManuallyActivated = Fan.State.IsOn());
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(HandleMotionDetected);
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(HandleMotionStopped);
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private void HandleMotionDetected(StateChange e)
    {
        if (IsFanManuallyActivated)
        {
            Fan.TurnOn();
        }
    }

    private void HandleMotionStopped(StateChange e) => Fan.TurnOff();
}
