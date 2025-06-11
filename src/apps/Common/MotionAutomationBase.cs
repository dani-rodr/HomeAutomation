using System.Collections.Generic;

namespace HomeAutomation.apps.Common;

public abstract class MotionAutomationBase(
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    LightEntity light,
    NumberEntity sensorDelay,
    ILogger logger
) : AutomationBase(logger, masterSwitch), IDisposable
{
    protected readonly BinarySensorEntity MotionSensor = motionSensor;
    protected readonly NumberEntity SensorDelay = sensorDelay;
    protected readonly LightEntity Light = light;
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorDelayValueActive => 5;
    protected virtual int SensorDelayValueInactive => 1;

    protected sealed override IEnumerable<IDisposable> GetStartupAutomations()
    {
        yield return Light.StateChanges().Subscribe(ControlMasterSwitchOnLightChange);
        if (MasterSwitch != null)
        {
            yield return MasterSwitch.StateChanges().IsOn().Subscribe(ControlLightOnMotionChange);
        }
        foreach (var automation in GetAdditionalStartupAutomations())
        {
            yield return automation;
        }
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
        [.. GetLightAutomations(), .. GetSensorDelayAutomations(), .. GetAdditionalSwitchableAutomations()];

    protected virtual IEnumerable<IDisposable> GetLightAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalStartupAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        yield return MotionSensor
            .StateChanges()
            .IsOnForSeconds(SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor
            .StateChanges()
            .IsOffForSeconds(SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
    }

    private void ControlMasterSwitchOnLightChange(StateChange evt)
    {
        var isLightsOn = Light.IsOn();
        var isManuallyOperated = HaIdentity.IsManuallyOperated(evt.UserId());
        if (!isManuallyOperated)
        {
            return;
        }
        if (isLightsOn)
        {
            Logger.LogInformation(
                "ControlMasterSwitchOnLightChange: Light turned ON by manual operation, turning OFF master switch."
            );
            MasterSwitch?.TurnOff();
        }
        else
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

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
