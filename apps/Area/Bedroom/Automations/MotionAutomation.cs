using System.Collections.Generic;
using System.Diagnostics;

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

    protected override IEnumerable<IDisposable> SwitchableAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => MotionDetected());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(_ => MotionStopped());
        yield return _leftSideFanSwitch.StateChanges().Subscribe(UpdateFanActivationStatus());
    }

    private Action<StateChange<SwitchEntity, EntityState<SwitchAttributes>>> UpdateFanActivationStatus()
    {
        return e =>
        {
            if (!HaIdentity.IsManuallyOperated(e.UserId()))
            {
                return;
            }
            _isFanManuallyActivated = e.State() == HaEntityStates.ON;
        };
    }

    private void MotionDetected()
    {
        Light.TurnOn();
        if (_isFanManuallyActivated)
        {
            _leftSideFanSwitch.TurnOn();
        }
    }

    private void MotionStopped()
    {
        Light.TurnOff();
        _leftSideFanSwitch.TurnOff();
    }
}
