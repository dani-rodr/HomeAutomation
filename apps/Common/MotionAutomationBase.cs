using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.apps.Common;

public abstract class MotionAutomationBase : IDisposable
{
    protected readonly BinarySensorEntity _motionSensor;
    protected readonly LightEntity _light;
    protected readonly SwitchEntity _enableSwitch;
    protected readonly NumberEntity _sensorDelay;
    protected abstract IEnumerable<IDisposable> GetAutomations();
    protected virtual int SensorWaitTime => 15;
    protected virtual int SensorDelayValueActive => 5;
    protected virtual int SensorDelayValueInactive => 1;

    private CompositeDisposable? _automations;
    private CancellationTokenSource? _cancelPendingLightTurnOff;

    protected bool ShouldDimLights(int dimThreshold) => (_sensorDelay.State ?? 0) > dimThreshold;

    protected virtual void OnMotionDetected()
    {
        CancelPendingTurnOff();
        _light.TurnOn(brightnessPct: 100);
    }

    protected virtual async Task OnMotionStoppedAsync(int dimBrightnessPct, int dimDelaySeconds)
    {
        if (!ShouldDimLights(dimDelaySeconds))
        {
            _light.TurnOff();
            return;
        }
        CancelPendingTurnOff();

        _cancelPendingLightTurnOff = new CancellationTokenSource();
        var token = _cancelPendingLightTurnOff.Token;

        try
        {
            _light.TurnOn(brightnessPct: dimBrightnessPct);
            await Task.Delay(TimeSpan.FromSeconds(dimDelaySeconds), token);
            if (!token.IsCancellationRequested)
            {
                _light.TurnOff();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    protected void CancelPendingTurnOff()
    {
        _cancelPendingLightTurnOff?.Cancel();
        _cancelPendingLightTurnOff?.Dispose();
        _cancelPendingLightTurnOff = null;
    }

    // Update constructor to accept BinarySensorEntity
    protected MotionAutomationBase(SwitchEntity enableSwitch, BinarySensorEntity motionSensor, LightEntity light, NumberEntity sensorDelay)
    {
        _enableSwitch = enableSwitch;
        _motionSensor = motionSensor;
        _light = light;
        _sensorDelay = sensorDelay;

        _enableSwitch.StateChanges().Subscribe(_ => InitializeAutomations());
        // Do not call UpdateAutomationsBasedOnSwitch() here!
    }

    /// <summary>
    /// Must be called at the end of the derived class constructor after all fields are initialized.
    /// </summary>
    protected void InitializeAutomations()
    {
        if (_enableSwitch.State != HaEntityStates.ON)
        {
            DisableAutomations();
            return;
        }

        EnableAutomations();
        UpdateLightStateBasedOnMotion();
    }

    private void UpdateLightStateBasedOnMotion()
    {
        switch (_motionSensor.State)
        {
            case HaEntityStates.ON: _light.TurnOn(); break;
            case HaEntityStates.OFF: _light.TurnOff(); break;
            case HaEntityStates.UNAVAILABLE:
            case HaEntityStates.UNKNOWN:
                break;
        }
    }

    private void EnableAutomations()
    {
        if (_automations != null)
        {
            return;
        }
        _automations = [.. GetAutomations()];
    }

    private void DisableAutomations()
    {
        _automations?.Dispose();
        _automations = null;
    }

    public virtual void Dispose()
    {
        CancelPendingTurnOff();
        DisableAutomations();
    }
}