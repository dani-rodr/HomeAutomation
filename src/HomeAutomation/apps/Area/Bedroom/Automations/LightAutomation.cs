using System.Linq;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class LightAutomation(
    IBedroomLightEntities entities,
    BedroomLightSettings settings,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private readonly SwitchEntity _rightSideEmptySwitch = entities.RightSideEmptySwitch;

    private readonly SwitchEntity _leftSideFanSwitch = entities.LeftSideFanSwitch;
    private readonly BedroomLightSettings _settings = settings;

    protected override int SensorActiveDelayValue => _settings.SensorActiveDelayValue;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [.. GetLightSwitchAutomations(), .. GetSensorDelayAutomations()];

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [
            MotionSensor.OnOccupied().Subscribe(_ => Light.TurnOn()),
            MotionSensor.OnCleared().Subscribe(_ => Light.TurnOff()),
        ];

    private IEnumerable<IDisposable> GetLightSwitchAutomations()
    {
        yield return _leftSideFanSwitch
            .OnDoubleClick(timeout: _settings.LightSwitchDoubleClickTimeoutSeconds)
            .Subscribe(e =>
            {
                ToggleLightsViaSwitch(e.First());
            });

        yield return _rightSideEmptySwitch
            .OnChanges(new(StartImmediately: false))
            .Subscribe(ToggleLightsViaSwitch);

        yield return Light
            .OnTurnedOn(new(StartImmediately: false))
            .IsSystemOperated()
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
