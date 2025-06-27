using System.Linq;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class MotionAutomation(IBedroomMotionEntities entities, IScheduler scheduler, ILogger logger)
    : MotionAutomationBase(entities, logger)
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.RightSideEmptySwitch;
    private readonly SwitchEntity _leftSideFanSwitch = entities.LeftSideFanSwitch;
    protected override int SensorActiveDelayValue => 45;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [.. GetLightSwitchAutomations(), .. GetSensorDelayAutomations()];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor
            .StateChanges()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    Light.TurnOn();
                }
                else if (e.IsOff())
                {
                    Light.TurnOff();
                }
            });
    }

    private IEnumerable<IDisposable> GetLightSwitchAutomations()
    {
        yield return _leftSideFanSwitch
            .StateChanges()
            .OnDoubleClick(timeout: 2, scheduler)
            .Subscribe(e =>
            {
                ToggleLightsViaSwitch(e.First());
            });
        yield return _rightSideEmptySwitch.StateChanges().Subscribe(ToggleLightsViaSwitch);
        yield return Light
            .StateChanges()
            .IsAutomated()
            .IsOn()
            .Subscribe(_ => MasterSwitch?.TurnOn());
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
