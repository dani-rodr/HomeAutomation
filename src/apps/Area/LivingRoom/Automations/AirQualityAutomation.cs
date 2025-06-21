namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityAutomation(IAirQualityEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    private readonly NumericSensorEntity _airQuality = entities.Pm25Sensor;
    private readonly SwitchEntity _ledStatus = entities.LedStatus;
    private SwitchEntity _supportingFan => Fans[1];
    private bool _activateSupportingFan = false;
    private bool _isCleaningAir = false;

    private const int WAIT_TIME = 10;
    private const int CLEAN_AIR_THRESHOLD = 7;
    private const int DIRTY_AIR_THRESHOLD = 75;

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return SubscribeToFanStateChanges();
        yield return SubscribeToAirQuality();
        yield return SubscribeToManualFanOperation();
        yield return SubscribeToSupportingFanIdle();
    }

    private IDisposable SubscribeToAirQuality() =>
        _airQuality
            .StateChangesWithCurrent()
            .WhenStateIsForSeconds(_ => true, WAIT_TIME)
            .Subscribe(e =>
            {
                var value = double.TryParse(e?.State(), out var parsed) ? parsed : 0;

                if (value > DIRTY_AIR_THRESHOLD)
                {
                    HandlePoorAirQuality();
                }
                else if (value > CLEAN_AIR_THRESHOLD)
                {
                    HandleModerateAirQuality();
                }
                else
                {
                    Fan.TurnOff();
                }
            });

    private void HandleModerateAirQuality()
    {
        Fan.TurnOn();

        if (_isCleaningAir && !_activateSupportingFan)
        {
            _supportingFan.TurnOff();
            _isCleaningAir = false;
            _activateSupportingFan = false;
            entities.LivingRoomSwitch.TurnOn();
        }
    }

    private void HandlePoorAirQuality()
    {
        if (_activateSupportingFan)
        {
            return;
        }
        _supportingFan.TurnOn();
        _isCleaningAir = true;
        _activateSupportingFan = false;
        entities.LivingRoomSwitch.TurnOff();
    }

    private IDisposable SubscribeToManualFanOperation() =>
        _supportingFan
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                _activateSupportingFan = true;
            });

    private IDisposable SubscribeToSupportingFanIdle() =>
        _supportingFan
            .StateChangesWithCurrent()
            .IsOffForMinutes(10)
            .Subscribe(_ =>
            {
                _activateSupportingFan = false;
            });

    private IDisposable SubscribeToFanStateChanges() =>
        Fan.StateChanges()
            .Subscribe(e =>
            {
                if (Fan.IsOn())
                {
                    _ledStatus.TurnOn();
                }
                else
                {
                    _ledStatus.TurnOff();
                }
            });
}
