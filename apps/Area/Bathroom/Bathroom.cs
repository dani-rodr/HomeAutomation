using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Bathroom;

[NetDaemonApp]
public class Bathroom : MotionAutomationBase
{
    public Bathroom(Entities entities, ILogger<Bathroom> logger)
        : base(
            entities.Switch.BathroomMotionSensor,
            entities.BinarySensor.BathroomPresenceSensors,
            entities.Light.BathroomLights,
            entities.Number.ZEsp32C62StillTargetDelay,
            logger
        )
    {
        StartAutomation();
    }

    public override void StartAutomation()
    {
        InitializeMotionAutomation();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
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
