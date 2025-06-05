using HomeAssistantGenerated;
using HomeAutomation.apps.Helpers;
using NetDaemon.HassModel.Entities;

namespace HomeAutomation.apps.Area.Kitchen;

[NetDaemonApp]
public class Kitchen
{
    private readonly BinarySensorEntity _motionSensor;
    private readonly BinarySensorEntity _powerPlug;
    private readonly LightEntity _light;
    private readonly NumberEntity _sensorDelay;
    private readonly SwitchEntity _enableMotionSensor;

    public Kitchen(Entities entities, IHaContext ha)
    {
        _motionSensor = entities.BinarySensor.KitchenMotionSensors;
        _powerPlug = entities.BinarySensor.SmartPlug3PowerExceedsThreshold;
        _light = entities.Light.RgbLightStrip;
        _sensorDelay = entities.Number.Ld2410Esp325StillTargetDelay;
        _enableMotionSensor = entities.Switch.KitchenMotionSensor;

        SetupMotionTriggeredLighting();
        SetupSensorDelayAdjustment();
        SetupMotionSensorReactivation();
    }

    /// <summary>
    /// Turns on the light after 5 seconds of motion and off after 10 seconds of no motion.
    /// </summary>
    private void SetupMotionTriggeredLighting()
    {
        _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, 5)
            .Subscribe(_ => _light.TurnOn());

        _motionSensor
            .StateChanges()
            .IsOff()
            .Subscribe(_ => _light.TurnOff());
    }

    /// <summary>
    /// Adjusts the still target delay if motion is sustained for 25 seconds or the power plug turns on.
    /// </summary>
    private void SetupSensorDelayAdjustment()
    {
        _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, 25)
            .Subscribe(_ => _sensorDelay.SetValue(15));
        _motionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, 30)
            .Subscribe(_ => _sensorDelay.SetValue(1));
        _powerPlug
            .StateChanges()
            .IsOn()
            .Subscribe(_ => _sensorDelay.SetValue(15));
    }

    /// <summary>
    /// Turns on the motion sensor enable switch after 1 hour of no motion.
    /// </summary>
    private void SetupMotionSensorReactivation()
    {
        _motionSensor
            .StateChanges()
            .WhenStateIsForHours(HaEntityStates.OFF, 1)
            .Subscribe(_ => _enableMotionSensor.TurnOn());
    }
}
