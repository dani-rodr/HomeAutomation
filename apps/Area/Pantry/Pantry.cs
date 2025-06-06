using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Pantry;

[NetDaemonApp]
public class Pantry : MotionAutomationBase
{
    protected override int SensorWaitTime => 10;
    private readonly BinarySensorEntity _miScalePresenceSensor;
    private readonly LightEntity _mirrorLight;
    public Pantry(Entities entities, ILogger<Pantry> logger)
        : base(entities.Switch.PantryMotionSensor,
               entities.BinarySensor.PantryMotionSensors,
               entities.Light.PantryLights,
               entities.Number.ZEsp32C63StillTargetDelay,
               logger)
    {
        _miScalePresenceSensor = entities.BinarySensor.Esp32PresenceBedroomMiScalePresence;
        _mirrorLight = entities.Light.ControllerRgbDf1c0d;

        StartAutomation();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        // Lighting automation
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(_ =>
        {
            Light.TurnOff();
            _mirrorLight.TurnOff();
        });
        yield return _miScalePresenceSensor.StateChanges().IsOn().Subscribe(_ => _mirrorLight.TurnOn());
        // Sensor delay automation
        yield return MotionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime).Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime).Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
    }

    public override void StartAutomation()
    {
        InitializeMotionAutomation();
    }
}
