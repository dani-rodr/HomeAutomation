using System.Linq;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityAutomation(IAirQualityEntities entities, ILogger logger)
    : FanAutomationBase(entities, logger)
{
    private readonly NumericSensorEntity _airQuality = entities.Pm25Sensor;
    private readonly SwitchEntity _ledStatus = entities.LedStatus;
    private SwitchEntity _supportingFan => Fans.Last();
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

    protected override void RunInitialActions()
    {
        Logger.LogDebug("Running initial air quality check");
        HandleAirQuality(_airQuality.State ?? 0);
    }

    private void HandleAirQuality(double airQuality)
    {
        if (airQuality > DIRTY_AIR_THRESHOLD)
        {
            HandlePoorAirQuality();
            return;
        }

        if (airQuality > CLEAN_AIR_THRESHOLD)
        {
            HandleModerateAirQuality();
            return;
        }

        Fan.TurnOff();
    }

    private IDisposable SubscribeToAirQuality() =>
        _airQuality
            .StateChanges()
            .Subscribe(e =>
            {
                var value = double.TryParse(e?.State(), out var parsed) ? parsed : 0;
                Logger.LogDebug("Air quality state changed → {ParsedValue}", value);
                HandleAirQuality(value);
            });

    private void HandleModerateAirQuality()
    {
        Fan.TurnOn();

        if (_isCleaningAir && !_activateSupportingFan)
        {
            Logger.LogInformation(
                "Exiting cleaning mode – turning off supporting fan and re-enabling living room switch"
            );
            _supportingFan.TurnOff();
            _isCleaningAir = false;
            _activateSupportingFan = false;
            entities.LivingRoomFanAutomation.TurnOn();
        }
    }

    private void HandlePoorAirQuality()
    {
        if (_activateSupportingFan)
        {
            Logger.LogDebug("Supporting fan manually activated – skipping automatic override");
            return;
        }

        Logger.LogInformation(
            "Entering cleaning mode – turning on supporting fan and disabling living room switch"
        );
        _supportingFan.TurnOn();
        _isCleaningAir = true;
        _activateSupportingFan = false;
        entities.LivingRoomFanAutomation.TurnOff();
    }

    private IDisposable SubscribeToManualFanOperation() =>
        _supportingFan
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                _activateSupportingFan = true;
                Logger.LogInformation("Manual fan operation detected – override flag enabled");
            });

    private IDisposable SubscribeToSupportingFanIdle() =>
        _supportingFan
            .StateChanges()
            .IsOffForMinutes(10)
            .Subscribe(_ =>
            {
                _activateSupportingFan = false;
                Logger.LogInformation("Supporting fan idle for 10 minutes – override flag cleared");
            });

    private IDisposable SubscribeToFanStateChanges() =>
        Fan.StateChanges()
            .Subscribe(e =>
            {
                if (Fan.IsOn())
                {
                    _ledStatus.TurnOn();
                    Logger.LogDebug("Main fan turned ON – LED status ON");
                }
                else
                {
                    _ledStatus.TurnOff();
                    Logger.LogDebug("Main fan turned OFF – LED status OFF");
                }
            });
}
