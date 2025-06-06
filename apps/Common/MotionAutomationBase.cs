using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.apps.Common;

public abstract class MotionAutomationBase(SwitchEntity enableSwitch, BinarySensorEntity motionSensor, LightEntity light, NumberEntity sensorDelay, ILogger logger) : AutomationBase(logger), IDisposable
{
    protected readonly BinarySensorEntity MotionSensor = motionSensor;
    protected readonly LightEntity Light = light;
    protected readonly SwitchEntity EnableSwitch = enableSwitch;
    protected readonly NumberEntity SensorDelay = sensorDelay;
    protected abstract IEnumerable<IDisposable> GetAutomations();
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorDelayValueActive => 5;
    protected virtual int SensorDelayValueInactive => 1;

    private CompositeDisposable? Automations;
    private CancellationTokenSource? LightTurnOffCancellationToken;

    private bool ShouldDimLights(int dimThreshold) => (SensorDelay.State ?? 0) > dimThreshold;
    protected void InitializeMotionAutomation()
    {
        ToggleMotionAutomationBasedOnSwitch();
        EnableSwitch.StateChanges().Subscribe(_ => ToggleMotionAutomationBasedOnSwitch());
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
            EnableSwitch.TurnOff();
        }
        else if (state == HaEntityStates.OFF && HaIdentity.IsKnownUser(userId))
        {
            EnableSwitch.TurnOn();
        }
    }

    private void ToggleMotionAutomationBasedOnSwitch()
    {
        if (EnableSwitch.State != HaEntityStates.ON)
        {
            DisableAutomations();
            return;
        }

        EnableAutomations();
        UpdateLightStateBasedOnMotion();
    }

    private void UpdateLightStateBasedOnMotion()
    {
        switch (MotionSensor.State)
        {
            case HaEntityStates.ON: Light.TurnOn(); break;
            case HaEntityStates.OFF: Light.TurnOff(); break;
            case HaEntityStates.UNAVAILABLE:
            case HaEntityStates.UNKNOWN:
                break;
        }
    }
    private void EnableAutomations()
    {
        if (Automations != null)
        {
            return;
        }
        Automations = [.. GetAutomations()];
    }

    private void DisableAutomations()
    {
        Automations?.Dispose();
        Automations = null;
    }

    public virtual void Dispose()
    {
        CancelPendingTurnOff();
        DisableAutomations();
        GC.SuppressFinalize(this);
    }
}