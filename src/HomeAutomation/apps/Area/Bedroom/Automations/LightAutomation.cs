using System.Linq;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class LightAutomation(IBedroomLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.RightSideEmptySwitch;
    private readonly SwitchEntity _leftSideFanSwitch = entities.LeftSideFanSwitch;
    protected override int SensorActiveDelayValue => 45;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [.. GetLightSwitchAutomations(), .. GetSensorDelayAutomations(), GetPantryAutomation()];

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.OnOccupied().Subscribe(_ => Light.TurnOn()),
            MotionSensor.OnCleared().Subscribe(_ => Light.TurnOff()),
        ];

    private IDisposable GetPantryAutomation() =>
        MotionSensor.OnCleared().Subscribe(_ => entities.PantryAutomation.TurnOn());

    private IEnumerable<IDisposable> GetLightSwitchAutomations()
    {
        yield return _leftSideFanSwitch
            .OnDoubleClick(timeout: 2)
            .Subscribe(e =>
            {
                ToggleLightsViaSwitch(e.First());
            });
        yield return _rightSideEmptySwitch.StateChanges().Subscribe(ToggleLightsViaSwitch);
        yield return Light
            .OnTurnedOn(new(ShouldCheckIfAutomated: true))
            .Subscribe(_ => MasterSwitch.TurnOn());
    }

    private void ToggleLightsViaSwitch(StateChange e)
    {
        if (!HaIdentity.IsPhysicallyOperated(e.UserId()))
        {
            return;
        }
        Light.Toggle();
        MasterSwitch.TurnOff();
    }
}
