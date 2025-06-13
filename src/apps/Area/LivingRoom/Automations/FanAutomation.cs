using System.Linq;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(ILivingRoomFanEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    protected override bool ShouldActivateFan { get; set; } = false;
    private SwitchEntity ExhaustFan => Fans[2];

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetSalaFanAutomations(), .. GetCeilingFanManualOperations()];

    private IEnumerable<IDisposable> GetCeilingFanManualOperations()
    {
        yield return Fan.StateChanges().IsOn().IsManuallyOperated().Subscribe(_ => ShouldActivateFan = true);
        yield return Fan.StateChanges().IsOff().IsManuallyOperated().Subscribe(_ => ShouldActivateFan = false);
        yield return Fan.StateChanges().IsOffForMinutes(15).Subscribe(_ => ShouldActivateFan = true);
    }

    private void TurnOnSalaFans(StateChange e)
    {
        if (ShouldActivateFan)
        {
            Fan.TurnOn();
        }

        if (entities.BedroomPresenceSensor.IsOff())
        {
            ExhaustFan.TurnOn();
        }
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations()
    {
        yield return MotionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnSalaFans);
        yield return MotionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffFans);
    }
}
