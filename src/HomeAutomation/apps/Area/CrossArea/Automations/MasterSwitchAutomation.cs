using HomeAutomation.apps.Area.Bedroom.Devices;

namespace HomeAutomation.apps.Area.CrossArea.Automations;

public class MasterSwitchAutomation(
    ICrossAreaEntities entites,
    MotionSensor bedroomMotionSensor,
    ILogger logger
) : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetAutomations() => [GetPantrySwitchAutomation()];

    private IDisposable GetPantrySwitchAutomation() =>
        bedroomMotionSensor.OnCleared().Subscribe(_ => entites.PantryAutomation.TurnOn());
}
