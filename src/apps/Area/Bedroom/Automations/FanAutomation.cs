namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class FanAutomation(IBedroomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [GetFanManualOperationAutomations()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensor.StateChanges().Subscribe(HandleMotionDetection);
    }
}
