namespace HomeAutomation.apps.Common.Base;

public abstract class MotionAutomationBase(
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    LightEntity light,
    ILogger logger,
    NumberEntity? sensorDelay = null
) : AutomationBase(logger, masterSwitch)
{
    protected readonly BinarySensorEntity MotionSensor = motionSensor;
    protected readonly NumberEntity? SensorDelay = sensorDelay;
    protected readonly LightEntity Light = light;
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorActiveDelayValue => 5;
    protected virtual int SensorInactiveDelayValue => 1;

    protected sealed override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return Light.StateChanges().IsManuallyOperated().Subscribe(ControlMasterSwitchOnLightChange);
        if (MasterSwitch != null)
        {
            yield return MasterSwitch.StateChanges().IsOn().Subscribe(ControlLightOnMotionChange);
        }
        foreach (var automation in GetAdditionalPersistentAutomations())
        {
            yield return automation;
        }
    }

    protected sealed override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [.. GetLightAutomations(), .. GetSensorDelayAutomations(), .. GetAdditionalSwitchableAutomations()];

    protected virtual IEnumerable<IDisposable> GetLightAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalPersistentAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        yield return MotionSensor
            .StateChanges()
            .IsOnForSeconds(SensorWaitTime)
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorActiveDelayValue));
        yield return MotionSensor
            .StateChanges()
            .IsOffForSeconds(SensorWaitTime)
            .Subscribe(_ => SensorDelay?.SetNumericValue(SensorInactiveDelayValue));
    }

    private void ControlMasterSwitchOnLightChange(StateChange evt)
    {
        var lightState = Light.IsOn();
        var motionState = MotionSensor.IsOccupied();

        Logger.LogDebug(
            "LightChange detected: Light.IsOn={Light}, MotionSensor.IsOccupied={Motion}",
            lightState,
            motionState
        );

        if (lightState == motionState)
        {
            Logger.LogDebug("Enabling automation via MasterSwitch (states match)");
            MasterSwitch?.TurnOn();
            return;
        }

        Logger.LogDebug("Disabling automation via MasterSwitch (states mismatch)");
        MasterSwitch?.TurnOff();
    }

    private void ControlLightOnMotionChange(StateChange evt)
    {
        if (MotionSensor.IsOn())
        {
            Light.TurnOn();
            return;
        }
        Light.TurnOff();
    }
}
