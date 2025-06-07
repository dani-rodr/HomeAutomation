using System.Threading;
using System.Threading.Tasks;

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
    protected readonly LightEntity Light = light;
    protected readonly NumberEntity SensorDelay = sensorDelay;
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorDelayValueActive => 5;
    protected virtual int SensorDelayValueInactive => 1;

    private CancellationTokenSource? LightTurnOffCancellationToken;

    private bool ShouldDimLights(int dimThreshold) => (SensorDelay.State ?? 0) > dimThreshold;

    public override void StartAutomation()
    {
        base.StartAutomation();
        Light.StateChanges().Subscribe(ControlMasterSwitchOnLightChange);
        MasterSwitch?.StateChangesWithCurrent().IsOn().Subscribe(ControlLightOnMotionChange());
    }

    protected virtual void OnMotionDetected()
    {
        CancelPendingTurnOff();
        Light.TurnOn(brightnessPct: 100);
    }

    protected virtual async Task OnMotionStoppedAsync(int dimBrightnessPct, int dimDelaySeconds)
    {
        if (!ShouldDimLights(dimDelaySeconds))
        {
            Light.TurnOff();
            return;
        }
        CancelPendingTurnOff();

        LightTurnOffCancellationToken = new CancellationTokenSource();
        var token = LightTurnOffCancellationToken.Token;

        try
        {
            Light.TurnOn(brightnessPct: dimBrightnessPct);
            await Task.Delay(TimeSpan.FromSeconds(dimDelaySeconds), token);
            if (!token.IsCancellationRequested)
            {
                Light.TurnOff();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    protected void CancelPendingTurnOff()
    {
        LightTurnOffCancellationToken?.Cancel();
        LightTurnOffCancellationToken?.Dispose();
        LightTurnOffCancellationToken = null;
    }

    private void ControlMasterSwitchOnLightChange(StateChange<LightEntity, EntityState<LightAttributes>> evt)
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

    private Action<StateChange> ControlLightOnMotionChange()
    {
        return e =>
        {
            if (MotionSensor.State.IsOn())
            {
                Light.TurnOn();
            }
            else
            {
                Light.TurnOff();
            }
        };
    }

    public override void Dispose()
    {
        CancelPendingTurnOff();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
