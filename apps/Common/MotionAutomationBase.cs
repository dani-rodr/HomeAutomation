using System.Collections.Generic;
using System.Threading;

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
    protected virtual LightEntity Light { get; } = light;
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorDelayValueActive => 5;
    protected virtual int SensorDelayValueInactive => 1;

    public override void StartAutomation()
    {
        base.StartAutomation();
        Light.StateChanges().Subscribe(ControlMasterSwitchOnLightChange);
        MasterSwitch?.StateChangesWithCurrent().IsOn().Subscribe(ControlLightOnMotionChange);
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
        [.. GetLightAutomations(), .. GetSensorDelayAutomations(), .. GetAdditionalSwitchableAutomations()];

    protected virtual IEnumerable<IDisposable> GetLightAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetAdditionalSwitchableAutomations() => [];

    protected virtual IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return MotionSensor
            .StateChanges()
            .WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime)
            .Subscribe(_ => SensorDelay.SetNumericValue(SensorDelayValueInactive));
    }

    private void ControlMasterSwitchOnLightChange(StateChange evt)
    {
        var state = Light.State;
        var userId = evt.UserId();
        if (state == HaEntityStates.ON && HaIdentity.IsManuallyOperated(userId))
        {
            Logger.LogInformation(
                "ControlMasterSwitchOnLightChange: Light turned ON by manual operation, turning OFF master switch."
            );
            MasterSwitch?.TurnOff();
        }
        else if (state == HaEntityStates.OFF && HaIdentity.IsManuallyOperated(userId))
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
