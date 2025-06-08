using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation(Entities entities, ILogger<Bathroom> logger)
    : DimmingMotionAutomationBase(
        entities.Switch.BathroomMotionSensor,
        entities.BinarySensor.BathroomPresenceSensors,
        entities.Light.BathroomLights,
        entities.Number.ZEsp32C62StillTargetDelay,
        logger
    )
{
    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected());
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await OnMotionStoppedAsync(dimBrightnessPct: 80, dimDelaySeconds: 5));
    }
}
