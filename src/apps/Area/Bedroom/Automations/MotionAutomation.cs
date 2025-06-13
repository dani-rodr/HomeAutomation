using System.Linq;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class MotionAutomation(IBedroomMotionEntities entities, ILogger logger)
    : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.RightSideEmptySwitch;
    private readonly SwitchEntity _leftSideFanSwitch = entities.LeftSideFanSwitch;

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
