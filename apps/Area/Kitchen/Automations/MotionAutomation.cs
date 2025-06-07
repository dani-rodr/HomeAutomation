using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class MotionAutomation(Entities entities, ILogger<Kitchen> logger)
    : MotionAutomationBase(
        entities.Switch.KitchenMotionSensor,
        entities.BinarySensor.KitchenMotionSensors,
        entities.Light.RgbLightStrip,
        entities.Number.Ld2410Esp325StillTargetDelay,
        logger
    )
{
    private readonly BinarySensorEntity _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;

    public override void StartAutomation()
    {
        base.StartAutomation();
        SetupMotionSensorReactivation();
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        // Lighting automation
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, 5)
            .Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff());

        // Sensor delay automation
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
        yield return _powerPlug
            .StateChanges()
            .IsOn()
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
    }

    private void SetupMotionSensorReactivation()
    {
        MotionSensor.StateChanges().WhenStateIsForHours(HaEntityStates.OFF, 1).Subscribe(_ => MasterSwitch?.TurnOn());
    }
}
