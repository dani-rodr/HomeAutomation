using System.Linq;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.Bedroom;

public class FanAutomation(IFanAutomationEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    protected override bool ShouldActivateFan { get; set; } = false;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return Fan.StateChangesWithCurrent()
            .IsManuallyOperated()
            .Subscribe(_ => ShouldActivateFan = Fan.State.IsOn());
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(HandleMotionDetected);
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(HandleMotionStopped);
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private void HandleMotionDetected(StateChange e)
    {
        if (ShouldActivateFan)
        {
            Fan.TurnOn();
        }
    }

    private void HandleMotionStopped(StateChange e) => Fan.TurnOff();
}
