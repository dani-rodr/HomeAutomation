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
        if (Light.IsOn())
        {
            Logger.LogInformation(
                "ControlMasterSwitchOnLightChange: Light turned ON by manual operation, turning OFF master switch."
            );
            MasterSwitch?.TurnOff();
        }
        else if (Light.IsOff())
        {
            Logger.LogInformation(
                "ControlMasterSwitchOnLightChange: Light turned OFF by manual operation, turning ON master switch."
            );
            MasterSwitch?.TurnOn();
        }
    }

    private void ControlLightOnMotionChange(StateChange evt)
    {
        if (MotionSensor.IsOn())
        {
            Light.TurnOn();
        }
        else
        {
            Light.TurnOff();
        }
    }
}
