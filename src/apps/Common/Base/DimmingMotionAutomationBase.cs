namespace HomeAutomation.apps.Common.Base;

public abstract class DimmingMotionAutomationBase(
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    LightEntity light,
    ILogger logger,
    NumberEntity sensorDelay
) : MotionAutomationBase(masterSwitch, motionSensor, light, logger, sensorDelay)
{
    protected abstract int DimBrightnessPct { get; }
    protected abstract int DimDelaySeconds { get; }
    private CancellationTokenSource? _lightTurnOffCancellationToken;

    private bool ShouldDimLights() => (SensorDelay?.State ?? 0) == SensorActiveDelayValue;

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(OnMotionDetected);
        yield return MotionSensor.StateChanges().IsOff().Subscribe(async _ => await OnMotionStoppedAsync());
    }

    protected virtual void OnMotionDetected(StateChange e)
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

        _lightTurnOffCancellationToken = new CancellationTokenSource();
        var token = _lightTurnOffCancellationToken.Token;

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
        _lightTurnOffCancellationToken?.Cancel();
        _lightTurnOffCancellationToken?.Dispose();
        _lightTurnOffCancellationToken = null;
    }

    public override void Dispose()
    {
        CancelPendingTurnOff();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
