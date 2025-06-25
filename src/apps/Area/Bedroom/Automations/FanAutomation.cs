using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class FanAutomation(IBedroomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities, logger)
{
    protected override IDisposable GetIdleOperationAutomations() => Disposable.Empty;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [.. base.GetPersistentAutomations(), GetMasterSwitchOffAutomation()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensor.StateChanges().Subscribe(HandleMotionDetection);
    }

    private IDisposable GetMasterSwitchOffAutomation() =>
        MasterSwitch!
            .StateChanges()
            .IsOff()
            .Subscribe(_ =>
            {
                Fan.TurnOff();
            });
}
