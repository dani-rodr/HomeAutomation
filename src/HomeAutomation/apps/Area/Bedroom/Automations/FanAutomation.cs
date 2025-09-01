using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class FanAutomation(IBedroomFanEntities entities, ILogger<FanAutomation> logger)
    : FanAutomationBase(entities, logger)
{
    protected override IDisposable GetIdleOperationAutomations() => Disposable.Empty;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return MainFan
            .OnChanges(options: new(ShouldCheckIfManuallyOperated: true))
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

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            MotionSensor.OnOccupied().Subscribe(e => TurnOnFans(e)),
            MotionSensor.OnCleared().Subscribe(e => TurnOffFans(e)),
        ];
}
