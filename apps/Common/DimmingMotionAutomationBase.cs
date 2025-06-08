using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.apps.Common;

public abstract class DimmingMotionAutomationBase(
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    LightEntity light,
    NumberEntity sensorDelay,
    ILogger logger
) : MotionAutomationBase(masterSwitch, motionSensor, light, sensorDelay, logger)
{
    protected abstract int DimBrightnessPct { get; }
    protected abstract int DimDelaySeconds { get; }
    private CancellationTokenSource? LightTurnOffCancellationToken;

    private bool ShouldDimLights() => (SensorDelay.State ?? 0) == SensorDelayValueActive;

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(async _ => await OnMotionStoppedAsync());
    }

    protected virtual void OnMotionDetected()
    {
        CancelPendingTurnOff();
        Light.TurnOn(brightnessPct: 100);
    }

    protected virtual async Task OnMotionStoppedAsync()
    {
        if (!ShouldDimLights())
        {
            Light.TurnOff();
            return;
        }
        CancelPendingTurnOff();

        LightTurnOffCancellationToken = new CancellationTokenSource();
        var token = LightTurnOffCancellationToken.Token;

        try
        {
            Light.TurnOn(brightnessPct: DimBrightnessPct);
            await Task.Delay(TimeSpan.FromSeconds(DimDelaySeconds), token);
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

    public override void Dispose()
    {
        CancelPendingTurnOff();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
