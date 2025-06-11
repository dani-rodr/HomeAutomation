using System.Linq;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class MotionAutomation(Entities entities, ILogger logger)
    : MotionAutomationBase(
        entities.Switch.BedroomMotionSensor,
        entities.BinarySensor.BedroomPresenceSensors,
        entities.Light.BedLights,
        logger,
        entities.Number.Esp32PresenceBedroomStillTargetDelay
    )
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.Switch.Sonoff1002352c401;
    private readonly SwitchEntity _leftSideFanSwitch = entities.Switch.Sonoff100238104e1;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() => GetLightSwitchAutomations();

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChangesWithCurrent().IsOff().Subscribe(_ => Light.TurnOff());
    }

    private IEnumerable<IDisposable> GetLightSwitchAutomations()
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
}
