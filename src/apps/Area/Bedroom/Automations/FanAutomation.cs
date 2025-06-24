using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class FanAutomation(IBedroomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities, logger)
{
    protected override IDisposable GetIdleOperationAutomations() => Disposable.Empty;

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensor.StateChanges().Subscribe(HandleMotionDetection);
    }
}
