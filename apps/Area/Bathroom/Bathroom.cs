using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Bathroom;

[NetDaemonApp]
public class Bathroom : MotionAutomationBase
{
    public Bathroom(Entities entities)
        : base(entities.Switch.BathroomMotionSensor,
               entities.BinarySensor.BathroomPresenceSensors,
               entities.Light.BathroomLights,
               entities.Number.ZEsp32C62StillTargetDelay)
    {
        InitializeMotionAutomation();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        // Lighting automation
        yield return _motionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected());
        yield return _motionSensor.StateChanges().IsOff().Subscribe(async _ => await OnMotionStoppedAsync(dimBrightnessPct: 80, dimDelaySeconds: 5));
        // Sensor delay automation
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueInactive));
    }
}