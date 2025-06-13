namespace HomeAutomation.apps.Common.Services;

public class DimmingLightController(NumberEntity sensorDelay) : IDimmingLightController
{
    private CancellationTokenSource? _lightTurnOffCancellationToken;
    private int _sensorActiveDelayValue = 5;
    private int _dimBrightnessPct = 80;
    private int _dimDelaySeconds = 5;

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
            light.TurnOn(brightnessPct: _dimBrightnessPct);
            await Task.Delay(TimeSpan.FromSeconds(_dimDelaySeconds), token);
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

    private bool ShouldDimLights() => (sensorDelay.State ?? 0) == _sensorActiveDelayValue;

    private void CancelPendingTurnOff()
    {
        _lightTurnOffCancellationToken?.Cancel();
        _lightTurnOffCancellationToken?.Dispose();
        _lightTurnOffCancellationToken = null;
    }

    public void SetDimParameters(int brightnessPct, int delaySeconds)
    {
        _dimBrightnessPct = brightnessPct;
        _dimDelaySeconds = delaySeconds;
    }

    public void SetSensorActiveDelayValue(int value)
    {
        _sensorActiveDelayValue = value;
    }

    public void Dispose()
    {
        CancelPendingTurnOff();
        GC.SuppressFinalize(this);
    }
}
