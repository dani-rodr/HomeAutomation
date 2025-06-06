using System.Collections.Generic;

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
    protected override IEnumerable<IDisposable> SwitchableAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff());
    }
}
