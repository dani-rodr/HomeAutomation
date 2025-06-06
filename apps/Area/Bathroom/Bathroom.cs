using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HomeAutomation.apps.Area.Bathroom;

[NetDaemonApp]
public class Bathroom : IDisposable
{
    private readonly SwitchEntity _enableMotionSensor;
    private readonly BinarySensorEntity _motionSensor;
    private readonly LightEntity _light;
    private readonly NumberEntity _sensorDelay;
    private CancellationTokenSource? _cancelPendingLightTurnOff;
    private List<IDisposable>? _motionSubscriptions;
    private bool ShouldDimLights() => (_sensorDelay.State ?? 0) > 2;

    public Bathroom(Entities entities, IHaContext ha)
    {
        _enableMotionSensor = entities.Switch.BathroomMotionSensor;
        _motionSensor = entities.BinarySensor.BathroomPresenceSensors;
        _light = entities.Light.BathroomLights;
        _sensorDelay = entities.Number.ZEsp32C62StillTargetDelay;

        _enableMotionSensor.StateChanges().Subscribe(_ => SetMotionAutomationsBySwitchState());
        SetMotionAutomationsBySwitchState();
    }

    private void SetMotionAutomationsBySwitchState()
    {
        if (_enableMotionSensor.State == HaEntityStates.ON)
        {
            EnableMotionAutomations();
            if (_motionSensor.State == HaEntityStates.ON)
            {
                _light.TurnOn(brightnessPct: 100);
            }
            else
            {
                _light.TurnOff();
            }
        }
        else if (_enableMotionSensor.State == HaEntityStates.OFF)
        {
            DisableMotionAutomations();
            _light.TurnOff();
        }
    }

    private void EnableMotionAutomations()
    {
        DisableMotionAutomations();
        _motionSubscriptions =
        [
            _motionSensor.StateChanges().IsOn().Subscribe(_ => OnMotionDetected()),
            _motionSensor.StateChanges().IsOff().Subscribe(async _ => await OnMotionStoppedAsync()),
            _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.ON, 15).Subscribe(_ => _sensorDelay.SetNumericValue(5)),
            _motionSensor.StateChanges().WhenStateIsForSeconds(HaEntityStates.OFF, 5).Subscribe(_ => _sensorDelay.SetNumericValue(1))
        ];
    }

    private void DisableMotionAutomations()
    {
        if (_motionSubscriptions != null)
        {
            foreach (var sub in _motionSubscriptions)
                sub.Dispose();
            _motionSubscriptions.Clear();
            _motionSubscriptions = null;
        }
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

    public void Dispose()
    {
        CancelPendingTurnOff();
        DisableMotionAutomations();
    }
}