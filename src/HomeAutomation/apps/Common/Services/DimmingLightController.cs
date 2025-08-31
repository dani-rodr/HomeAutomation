using System.Reactive.Threading.Tasks;

namespace HomeAutomation.apps.Common.Services;

public class DimmingLightController(
    NumberEntity sensorDelay,
    ILogger<DimmingLightController> logger
) : IDimmingLightController
{
    private CancellationTokenSource? _lightTurnOffCancellationToken;
    private int _sensorActiveDelayValue = 5;
    private int _dimBrightnessPct = 80;
    private int _dimDelaySeconds = 5;

    public void OnMotionDetected(LightEntity light)
    {
        logger.LogDebug(
            "Motion detected for {EntityId} - canceling pending turn-off and setting brightness to 100%",
            light.EntityId
        );
        CancelPendingTurnOff();
        light.TurnOn(brightnessPct: 100);
    }

    public async Task OnMotionStoppedAsync(LightEntity light)
    {
        var shouldDim = ShouldDimLights();

        logger.LogDebug(
            "Motion stopped for {EntityId} - ShouldDim={ShouldDim} (SensorDelay={CurrentDelay}, ActiveValue={ActiveValue})",
            light.EntityId,
            shouldDim,
            sensorDelay.State ?? 0,
            _sensorActiveDelayValue
        );

        if (!shouldDim)
        {
            logger.LogDebug(
                "Motion stopped for {EntityId} - turning off immediately (dimming disabled)",
                light.EntityId
            );
            light.TurnOff();
            return;
        }

        logger.LogDebug(
            "Motion stopped for {EntityId} - starting dimming sequence: {Brightness}% for {DelaySeconds}s",
            light.EntityId,
            _dimBrightnessPct,
            _dimDelaySeconds
        );

        CancelPendingTurnOff();

        _lightTurnOffCancellationToken = new CancellationTokenSource();
        var token = _lightTurnOffCancellationToken.Token;

        try
        {
            light.TurnOn(brightnessPct: _dimBrightnessPct);
            logger.LogDebug(
                "Dimming sequence: {EntityId} dimmed to {Brightness}%, waiting {DelaySeconds}s before turning off",
                light.EntityId,
                _dimBrightnessPct,
                _dimDelaySeconds
            );

            await Observable
                .Timer(TimeSpan.FromSeconds(_dimDelaySeconds), SchedulerProvider.Current)
                .TakeUntil(token.AsObservable())
                .ToTask(token);
            if (!token.IsCancellationRequested)
            {
                logger.LogDebug(
                    "Dimming sequence completed for {EntityId} - turning off light",
                    light.EntityId
                );
                light.TurnOff();
            }
            else
            {
                logger.LogDebug(
                    "Dimming sequence cancelled for {EntityId} (new motion detected)",
                    light.EntityId
                );
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogDebug(
                "Dimming sequence cancelled for {EntityId} due to task cancellation",
                light.EntityId
            );
        }
    }

    private bool ShouldDimLights()
    {
        bool shouldDimWhenStateIsUnknown = !sensorDelay.State.HasValue;
        return shouldDimWhenStateIsUnknown || sensorDelay.State == _sensorActiveDelayValue;
    }

    private void CancelPendingTurnOff()
    {
        if (_lightTurnOffCancellationToken != null)
        {
            logger.LogDebug("Cancelling pending light turn-off operation");
            _lightTurnOffCancellationToken.Cancel();
            _lightTurnOffCancellationToken.Dispose();
            _lightTurnOffCancellationToken = null;
        }
    }

    public void SetDimParameters(int brightnessPct, int delaySeconds)
    {
        logger.LogDebug(
            "Updating dim parameters: Brightness {OldBrightness}% → {NewBrightness}%, Delay {OldDelay}s → {NewDelay}s",
            _dimBrightnessPct,
            brightnessPct,
            _dimDelaySeconds,
            delaySeconds
        );
        _dimBrightnessPct = brightnessPct;
        _dimDelaySeconds = delaySeconds;
    }

    public void SetSensorActiveDelayValue(int value)
    {
        logger.LogDebug(
            "Updating sensor active delay value: {OldValue} → {NewValue}",
            _sensorActiveDelayValue,
            value
        );
        _sensorActiveDelayValue = value;
    }

    public void Dispose()
    {
        CancelPendingTurnOff();
        GC.SuppressFinalize(this);
    }
}
