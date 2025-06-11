using System.Collections.Generic;
using apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class FanAutomation(Entities entities, ILogger<LivingRoom> logger)
    : AutomationBase(logger, entities.Switch.SalaMotionSensor)
{
    private readonly SwitchEntity _ceilingFan = entities.Switch.CeilingFan;
    private readonly SwitchEntity _standFan = entities.Switch.Sonoff10023810231;
    private readonly SwitchEntity _exhaustFan = entities.Switch.Cozylife955f;
    private readonly BinarySensorEntity _motionSensor = entities.BinarySensor.LivingRoomPresenceSensors;
    private bool _isCeilingFanManuallyOff = false;

    public override void StartAutomation()
    {
        base.StartAutomation();
        AirPurifier airPurifier = new(entities, logger);
        airPurifier.StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetStartupAutomations() => [];

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
        [.. GetSalaFanAutomations(), GetCeilingFanManualOperations()];

    private IDisposable GetCeilingFanManualOperations()
    {
        return _ceilingFan.StateChanges().IsOff().IsManuallyOperated().Subscribe(_ => _isCeilingFanManuallyOff = true);
    }

    private void TurnOnSalaFans(StateChange e)
    {
        if (!_isCeilingFanManuallyOff)
        {
            _ceilingFan.TurnOn();
        }

        if (entities.BinarySensor.BedroomPresenceSensors.IsOff())
        {
            _exhaustFan.TurnOn();
        }
    }

    private void TurnOffAllFans(StateChange e)
    {
        _ceilingFan.TurnOff();
        _standFan.TurnOff();
        _exhaustFan.TurnOff();
    }

    private IEnumerable<IDisposable> GetSalaFanAutomations()
    {
        yield return _motionSensor.StateChanges().IsOnForSeconds(3).Subscribe(TurnOnSalaFans);
        yield return _motionSensor.StateChanges().IsOffForMinutes(1).Subscribe(TurnOffAllFans);
    }
}
