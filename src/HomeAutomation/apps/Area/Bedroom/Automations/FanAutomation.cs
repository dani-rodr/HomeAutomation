using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class FanAutomation(IBedroomFanEntities entities, ILogger<FanAutomation> logger)
    : FanAutomationBase(entities, logger)
{
    protected override IDisposable GetIdleOperationAutomations() => Disposable.Empty;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return MainFan
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(_ =>
            {
                if (MainFan.IsOn() && MotionSensor.IsOn())
                {
                    MasterSwitch.TurnOn();
                    return;
                }
                MasterSwitch.TurnOff();
            });
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensor.StateChanges().Subscribe(HandleMotionDetection);
    }
}
