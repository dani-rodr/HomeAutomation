namespace HomeAutomation.apps.Area.Bedroom;

public class FanAutomation(IFanEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    protected override bool ShouldActivateFan { get; set; } = false;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return Fan.StateChanges().IsManuallyOperated().Subscribe(_ => ShouldActivateFan = Fan.State.IsOn());
        yield return MotionSensor.StateChanges().Subscribe(HandleMotionDetection);
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private void HandleMotionDetection(StateChange e)
    {
        if (e.IsOn() && ShouldActivateFan)
        {
            Fan.TurnOn();
        }
        else if (e.IsOff())
        {
            Fan.TurnOff();
        }
    }
}
