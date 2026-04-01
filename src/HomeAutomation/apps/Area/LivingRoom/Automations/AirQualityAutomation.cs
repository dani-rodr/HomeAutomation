using System.Reactive.Disposables;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityAutomation(
    IAirQualityEntities entities,
    LivingRoomAirQualitySettings settings,
    ILogger<AirQualityAutomation> logger
) : FanAutomationBase(entities, logger)
{
    private readonly NumericSensorEntity _airQuality = entities.Pm25Sensor;

    private readonly SwitchEntity _ledStatus = entities.LedStatus;

    private readonly SwitchEntity _supportingFan = entities.SupportingFan;

    private bool _activateSupportingFan = false;

    private bool _isCleaningAir = false;

    private bool _wasSalaFanAutomationTurnedOff = false;
    private readonly LivingRoomAirQualitySettings _settings = settings;

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return SubscribeToMainFanStateChanges();

        yield return SubscribeToAirQuality();

        yield return SubscribeToManualFanOperation();

        yield return SubscribeToSupportingFanIdle();
    }

    protected override IDisposable GetMasterSwitchAutomations() => Disposable.Empty;

    protected override void RunInitialActions()
    {
        Logger.LogDebug("Running initial air quality check");

        HandleAirQuality(_airQuality.State ?? 0);
    }

    private void HandleAirQuality(double airQuality)
    {
        var airQualitySettings = _settings;

        if (airQuality > airQualitySettings.DirtyThresholdPm25)
        {
            HandlePoorAirQuality();

            return;
        }

        if (airQuality > airQualitySettings.CleanThresholdPm25)
        {
            HandleModerateAirQuality();

            return;
        }

        MainFan.TurnOff();
    }

    private IDisposable SubscribeToAirQuality() =>
        _airQuality
            .OnChanges()
            .Subscribe(e =>
            {
                var value = _airQuality.State ?? 0;

                Logger.LogDebug("Air quality state changed → {ParsedValue}", value);

                HandleAirQuality(value);
            });

    private void HandleModerateAirQuality()
    {
        MainFan.TurnOn();

        if (_isCleaningAir && !_activateSupportingFan)
        {
            Logger.LogInformation(
                "Exiting cleaning mode - turning off supporting fan and re-enabling living room switch"
            );

            _supportingFan.TurnOff();

            _isCleaningAir = false;

            _activateSupportingFan = false;

            if (_wasSalaFanAutomationTurnedOff)
            {
                entities.LivingRoomFanAutomation.TurnOn();

                _wasSalaFanAutomationTurnedOff = false;
            }
        }
    }

    private void HandlePoorAirQuality()
    {
        if (_activateSupportingFan)
        {
            Logger.LogDebug("Supporting fan manually activated - skipping automatic override");

            return;
        }

        Logger.LogInformation(
            "Entering cleaning mode - turning on supporting fan and disabling living room switch"
        );

        _supportingFan.TurnOn();

        _isCleaningAir = true;

        _activateSupportingFan = false;

        if (entities.LivingRoomFanAutomation.IsOn())
        {
            _wasSalaFanAutomationTurnedOff = true;

            entities.LivingRoomFanAutomation.TurnOff();
        }
    }

    private IDisposable SubscribeToManualFanOperation() =>
        _supportingFan
            .OnChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                _activateSupportingFan = true;

                Logger.LogInformation("Manual fan operation detected - override flag enabled");
            });

    private IDisposable SubscribeToSupportingFanIdle() =>
        _supportingFan
            .OnTurnedOff(new(Minutes: _settings.ManualOverrideResetMinutes))
            .Subscribe(_ =>
            {
                _activateSupportingFan = false;

                Logger.LogInformation("Supporting fan idle - override flag cleared");
            });

    private IDisposable SubscribeToMainFanStateChanges() =>
        MainFan
            .OnChanges()
            .Subscribe(e =>
            {
                if (MainFan.IsOn())
                {
                    _ledStatus.TurnOn();

                    Logger.LogDebug("Main fan turned ON - LED status ON");
                }
                else
                {
                    _ledStatus.TurnOff();

                    Logger.LogDebug("Main fan turned OFF - LED status OFF");
                }
            });
}
