using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Bathroom.Automations;

public class MotionAutomation(Entities entities, ILogger<Bathroom> logger)
    : MotionAutomationBase(
        entities.Switch.PantryMotionSensor,
        entities.BinarySensor.PantryMotionSensors,
        entities.Light.PantryLights,
        entities.Number.ZEsp32C63StillTargetDelay,
        logger
    )
{
    protected override IEnumerable<IDisposable> SwitchableAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected());
        yield return MotionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await OnMotionStoppedAsync(dimBrightnessPct: 80, dimDelaySeconds: 5));
        // Sensor delay automation
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
    }
}
