using System.Reactive.Concurrency;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(Entities entities, ILogger logger) : AutomationBase(logger)
{
    private readonly NumericSensorEntity _riceCookerPower = entities.Sensor.RiceCookerPower;
    private readonly SwitchEntity _riceCookerSwitch = entities.Switch.RiceCookerSocket1;
    private readonly SensorEntity _airFryerStatus = entities.Sensor.CareliSg593061393Maf05aStatusP21;
    private readonly ButtonEntity _inductionTurnOff = entities.Button.InductionCookerPower;
    private readonly BinarySensorEntity _cookingWattageExceedThreshold = entities
        .BinarySensor
        .SmartPlug3PowerExceedsThreshold;

    public override void StartAutomation()
    {
        AutoTurnOffRiceCookerOnIdle(minutes: 10);
        AutoTurnOffInductionOnIdle(minutes: 12);
    }

    private void AutoTurnOffInductionOnIdle(int minutes)
    {
        _cookingWattageExceedThreshold
            .StateChangesWithCurrent()
            .WhenStateIsForMinutes(HaEntityStates.ON, minutes)
            .Subscribe(_ =>
            {
                if (_airFryerStatus.State == HaEntityStates.UNAVAILABLE)
                {
                    _inductionTurnOff.Press();
                }
            });
    }

    private void AutoTurnOffRiceCookerOnIdle(int minutes)
    {
        var riceCookerIdlePower = 100;
        _riceCookerPower
            .StateChangesWithCurrent()
            .WhenStateIsForMinutes(s => s?.State < riceCookerIdlePower, minutes)
            .Subscribe(_ => _riceCookerSwitch.TurnOff());
    }
}
