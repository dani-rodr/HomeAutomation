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
        Light.StateChanges().Subscribe(e => HandleLightToggleSwitch(e));
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

    private void HandleLightToggleSwitch(StateChange<LightEntity, EntityState<LightAttributes>> evt)
    {
        var state = evt.New?.State;
        var userId = evt.New?.Context?.UserId;
        if (state == HaEntityStates.ON && HaIdentity.IsKnownUser(userId))
        {
            MasterSwitch?.TurnOff();
        }
        else if (state == HaEntityStates.OFF && HaIdentity.IsKnownUser(userId))
        {
            MasterSwitch?.TurnOn();
        }
    }

    protected override void ToggleAutomation()
    {
        base.ToggleAutomation();
        UpdateLightStateBasedOnMotion();
    }

    private void UpdateLightStateBasedOnMotion()
    {
        switch (MotionSensor.State)
        {
            case HaEntityStates.ON:
                Light.TurnOn();
                break;
            case HaEntityStates.OFF:
                Light.TurnOff();
                break;
            case HaEntityStates.UNAVAILABLE:
            case HaEntityStates.UNKNOWN:
                break;
        }
    }

    public override void Dispose()
    {
        CancelPendingTurnOff();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
