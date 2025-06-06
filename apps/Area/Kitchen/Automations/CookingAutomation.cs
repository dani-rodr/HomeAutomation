using System.Reactive.Concurrency;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class CookingAutomation(Entities entities, ILogger logger) : AutomationBase(logger)
{
    private readonly NumericSensorEntity _riceCookerPower = entities.Sensor.RiceCookerPower;
    private readonly SwitchEntity _riceCookerSwitch = entities.Switch.RiceCookerSocket1;
    private readonly BinarySensorEntity _powerThresholdExceeded = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;

    public override void StartAutomation()
    {
        _riceCookerPower
            .StateChangesWithCurrent()
            .WhenStateIsForMinutes(s => s?.State < 100, 10)
            .Subscribe(_ => _riceCookerSwitch.TurnOff());
    }
}
