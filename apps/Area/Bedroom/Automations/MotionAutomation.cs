using System.Collections.Generic;
using System.Linq;

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
    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations() => [.. SetupLightSwitchAutomations(), .. SetupFanMotionAutomations()];

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(_ => Light.TurnOff());
    }

    private IEnumerable<IDisposable> SetupLightSwitchAutomations()
    {
        yield return _leftSideFanSwitch
            .StateChanges()
            .OnDoubleClick(timeout: 2)
            .Subscribe(e =>
            {
                ToggleLightsViaSwitch(e.First());
            });
        yield return _rightSideEmptySwitch.StateChanges().Subscribe(ToggleLightsViaSwitch);
        yield return Light.StateChanges().Subscribe(EnableMasterSwitchWhenLightActive);
    }

    private IEnumerable<IDisposable> SetupFanMotionAutomations()
    {
        yield return _leftSideFanSwitch.StateChangesWithCurrent().Subscribe(UpdateFanActivationStatus);
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(HandleMotionDetected);
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(HandleMotionStopped);
    }

    private void EnableMasterSwitchWhenLightActive(StateChange e)
    {
        var state = Light.State;
        if (HaIdentity.IsAutomated(e.UserId()) && state.IsOn())
        {
            MasterSwitch?.TurnOn();
        }
    }

    private void ToggleLightsViaSwitch(StateChange e)
    {
        if (!HaIdentity.IsPhysicallyOperated(e.UserId()))
        {
            return;
        }
        Light.Toggle();
        MasterSwitch?.TurnOff();
    }

    private void UpdateFanActivationStatus(StateChange e)
    {
        if (!HaIdentity.IsManuallyOperated(e.UserId()))
        {
            return;
        }
        _isFanManuallyActivated = _leftSideFanSwitch.State.IsOn();
    }

    private void HandleMotionDetected(StateChange e)
    {
        if (_isFanManuallyActivated)
        {
            _leftSideFanSwitch.TurnOn();
        }
    }

    private void HandleMotionStopped(StateChange e) => _leftSideFanSwitch.TurnOff();
}
