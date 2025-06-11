using System.Collections.Generic;

namespace HomeAutomation.apps.Area.Kitchen.Automations;

public class MotionAutomation(Entities entities, ILogger logger)
    : MotionAutomationBase(
        entities.Switch.KitchenMotionSensor,
        entities.BinarySensor.KitchenMotionSensors,
        entities.Light.RgbLightStrip,
        entities.Number.Ld2410Esp325StillTargetDelay,
        logger
    )
{
    private readonly BinarySensorEntity _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOnForSeconds(5).Subscribe(_ => Light.TurnOn());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff());
    }

    protected override IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        return
        [
            .. base.GetSensorDelayAutomations(),
            _powerPlug.StateChanges().IsOn().Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive)),
        ];
    }

    protected override IEnumerable<IDisposable> GetAdditionalStartupAutomations() => [SetupMotionSensorReactivation()];

    private IDisposable SetupMotionSensorReactivation()
    {
        return MotionSensor.StateChanges().IsOffForHours(1).Subscribe(_ => MasterSwitch?.TurnOn());
    }
}
