using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.apps.Area.Bathroom;

[NetDaemonApp]
public class Bathroom : MotionAutomationBase
{
    private readonly BinarySensorEntity _motionSensor;
    private readonly LightEntity _light;
    private readonly NumberEntity _sensorDelay;
    private CancellationTokenSource? _cancelPendingLightTurnOff;
    private bool ShouldDimLights() => (_sensorDelay.State ?? 0) > 2;

    public Bathroom(Entities entities, IHaContext ha)
        : base(entities.Switch.BathroomMotionSensor)
    {
        _motionSensor = entities.BinarySensor.BathroomPresenceSensors;
        _light = entities.Light.BathroomLights;
        _sensorDelay = entities.Number.ZEsp32C62StillTargetDelay;

        UpdateAutomationsBasedOnSwitch();
    }

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        const int SensorWaitTime = 15;
        const int SensorDelayValueActive = 5;
        const int SensorDelayValueInactive = 1;
        // Lighting automation
        yield return _motionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected());
        yield return _motionSensor.StateChanges().IsOff().Subscribe(async _ => await OnMotionStoppedAsync());
        // Sensor delay automation
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueActive));
        yield return _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, SensorWaitTime).Subscribe(_ => _sensorDelay.SetNumericValue(SensorDelayValueInactive));
    }

    private void OnMotionDetected()
    {
        CancelPendingTurnOff();
        _light.TurnOn(brightnessPct: 100);
    }

    private async Task OnMotionStoppedAsync()
    {
        if (!ShouldDimLights())
        {
            _light.TurnOff();
            return;
        }
        CancelPendingTurnOff();

        _cancelPendingLightTurnOff = new CancellationTokenSource();
        var token = _cancelPendingLightTurnOff.Token;

        try
        {
            _light.TurnOn(brightnessPct: 80);
            await Task.Delay(TimeSpan.FromSeconds(5), token);
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

    private void CancelPendingTurnOff()
    {
        _cancelPendingLightTurnOff?.Cancel();
        _cancelPendingLightTurnOff?.Dispose();
        _cancelPendingLightTurnOff = null;
    }

    public override void Dispose()
    {
        CancelPendingTurnOff();
        base.Dispose();
    }
}