using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class MotionAutomation(Entities entities, ILogger<Bedroom> logger)
    : MotionAutomationBase(
        entities.Switch.BedroomMotionSensor,
        entities.BinarySensor.BedroomPresenceSensors,
        entities.Light.BedLights,
        entities.Number.Esp32PresenceBedroomStillTargetDelay,
        logger
    )
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.Switch.Sonoff1002352c401;
    private readonly SwitchEntity _leftSideFanSwitch = entities.Switch.Sonoff100238104e1;
    private bool _isFanManuallyActivated = false;

    public override void StartAutomation()
    {
        base.StartAutomation();
        _rightSideEmptySwitch.StateChangesWithCurrent().Subscribe(ToggleLightsViaSwitch());

        // Fan automation
        _leftSideFanSwitch.StateChangesWithCurrent().Subscribe(UpdateFanActivationStatus());
        MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(MotionDetected());
        MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(MotionStopped());
        Light.StateChangesWithCurrent().Subscribe(EnableMasterSwitchWhenLightActive());
    }

    private Action<StateChange> EnableMasterSwitchWhenLightActive()
    {
        return e =>
        {
            var state = Light.State;
            var isAutomated = HaIdentity.IsAutomated(e.UserId());
            if (isAutomated && state.IsOn())
            {
                MasterSwitch?.TurnOn();
                return;
            }
        };
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(_ => Light.TurnOff());
    }

    private Action<StateChange> ToggleLightsViaSwitch()
    {
        return e =>
        {
            if (!HaIdentity.IsPhysicallyOperated(e.UserId()))
            {
                return;
            }
            Light.Toggle();
            MasterSwitch?.TurnOff();
        };
    }

    private Action<StateChange> UpdateFanActivationStatus()
    {
        return e =>
        {
            if (!HaIdentity.IsManuallyOperated(e.UserId()))
            {
                return;
            }
            _isFanManuallyActivated = _leftSideFanSwitch.State.IsOn();
        };
    }

    private Action<StateChange> MotionDetected()
    {
        return _ =>
        {
            if (_isFanManuallyActivated)
            {
                _leftSideFanSwitch.TurnOn();
            }
        };
    }

    private Action<StateChange> MotionStopped()
    {
        return _ =>
        {
            _leftSideFanSwitch.TurnOff();
        };
    }
}
