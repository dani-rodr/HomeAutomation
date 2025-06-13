namespace HomeAutomation.apps.Common.Services;

public class DimmingLightController(
    int sensorActiveDelayValue,
    NumberEntity sensorDelay,
    int dimBrightnessPct = 80,
    int dimDelaySeconds = 5
) : IDisposable
{
    private CancellationTokenSource? _lightTurnOffCancellationToken;

    public void OnMotionDetected(LightEntity light)
    {
        CancelPendingTurnOff();
        light.TurnOn(brightnessPct: 100);
    }

    public async Task OnMotionStoppedAsync(LightEntity light)
    {
        if (!ShouldDimLights())
        {
            light.TurnOff();
            return;
        }
        CancelPendingTurnOff();

        _lightTurnOffCancellationToken = new CancellationTokenSource();
        var token = _lightTurnOffCancellationToken.Token;

        try
        {
            light.TurnOn(brightnessPct: dimBrightnessPct);
            await Task.Delay(TimeSpan.FromSeconds(dimDelaySeconds), token);
            if (!token.IsCancellationRequested)
            {
                light.TurnOff();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
    }

    private bool ShouldDimLights() => (sensorDelay.State ?? 0) == sensorActiveDelayValue;

    private void CancelPendingTurnOff()
    {
        _lightTurnOffCancellationToken?.Cancel();
        _lightTurnOffCancellationToken?.Dispose();
        _lightTurnOffCancellationToken = null;
    }

    public void Dispose()
    {
        CancelPendingTurnOff();
        GC.SuppressFinalize(this);
    }
}
