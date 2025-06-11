using apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(Entities entities, ILogger logger)
    : FanAutomationBase(
        entities.Switch.SalaMotionSensor,
        entities.BinarySensor.LivingRoomPresenceSensors,
        logger,
        entities.Switch.CeilingFan,
        entities.Switch.Sonoff10023810231,
        entities.Switch.Cozylife955f
    )
{
    private SwitchEntity _exhaustFan = entities.Switch.Cozylife955f;

    protected override bool IsFanManuallyActivated { get; set; } = false;

    public override void StartAutomation()
    {
        base.StartAutomation();
        AirPurifier airPurifier = new(entities, Logger);
        airPurifier.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetSalaFanAutomations(), .. GetCeilingFanManualOperations()];

    private IEnumerable<IDisposable> GetCeilingFanManualOperations()
    {
        yield return Fan.StateChanges().IsOn().IsManuallyOperated().Subscribe(_ => IsFanManuallyActivated = true);
        yield return Fan.StateChanges().IsOff().IsManuallyOperated().Subscribe(_ => IsFanManuallyActivated = false);
        yield return Fan.StateChanges().IsOffForMinutes(15).Subscribe(_ => IsFanManuallyActivated = true);
    }

    private void TurnOnSalaFans(StateChange e)
    {
        if (IsFanManuallyActivated)
        {
            Fan.TurnOn();
        }

        if (entities.BinarySensor.BedroomPresenceSensors.IsOff())
        {
            _exhaustFan.TurnOn();
        }
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations()
    {
        yield return MotionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnSalaFans);
        yield return MotionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffFans);
    }
}
