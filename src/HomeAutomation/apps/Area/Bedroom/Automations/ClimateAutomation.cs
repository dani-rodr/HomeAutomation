using System.Linq;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(
    IClimateEntities entities,
    IClimateAutomationScheduler climateAutomationScheduler,
    ILogger<ClimateAutomation> logger
) : ToggleableAutomation(entities.MasterSwitch, logger)
{
    private readonly ClimateEntity _ac = entities.AirConditioner;
    private readonly SwitchEntity _fanAutomation = entities.FanAutomation;

    private readonly BinarySensorEntity _motionSensor = entities.MotionSensor;

    private readonly BinarySensorEntity _doorSensor = entities.Door;

    private readonly InputBooleanEntity _powerSavingMode = entities.PowerSavingMode;

    private readonly WeatherEntity _weather = entities.Weather;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        var automationSettings = climateAutomationScheduler.GetCurrentSettings().Automation;

        yield return climateAutomationScheduler.GetResetSchedule();

        yield return _weather.StateAllChanges().Subscribe(ApplyPowerSavingModeFromWeather);

        yield return climateAutomationScheduler.Changes.Subscribe(HandleBedroomSettingsChanged);

        yield return _motionSensor
            .OnCleared(new(Hours: automationSettings.MasterSwitchReenableWhenNoMotionHours))
            .Where(_ => MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch.TurnOn());

        yield return MasterSwitch
            .OnTurnedOff(new(Hours: automationSettings.MasterSwitchReenableAfterOffHours))
            .Subscribe(_ => MasterSwitch.TurnOn());

        yield return _doorSensor
            .OnClosed()
            .Where(_ => MasterSwitch.IsOn())
            .Subscribe(e => ApplyTimeBasedAcSetting(e));

        yield return MasterSwitch.OnTurnedOn().Subscribe(e => ApplyTimeBasedAcSetting(e));
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            .. climateAutomationScheduler.GetSchedules(() =>
            {
                ApplyScheduledAcSettings();
            }),
            .. GetSensorBasedAutomations(),
            .. GetHousePresenceAutomations(),
            .. GetFanModeToggleAutomation(),
        ];

    private IEnumerable<IDisposable> GetSensorBasedAutomations()
    {
        var automationSettings = climateAutomationScheduler.GetCurrentSettings().Automation;

        yield return _doorSensor
            .OnOpened(new(Minutes: automationSettings.DoorOpenReapplyMinutes))
            .Subscribe(e => ApplyTimeBasedAcSetting(e));

        yield return _motionSensor
            .OnCleared(new(Minutes: automationSettings.MotionClearedReapplyMinutes))
            .Subscribe(e => ApplyTimeBasedAcSetting(e));

        yield return _motionSensor.OnOccupied().Subscribe(e => ApplyTimeBasedAcSetting(e));

        yield return _powerSavingMode
            .OnChanges()
            .Subscribe(e => ApplyTimeBasedAcSetting(e, allowFanAssistEnable: false));
    }

    private void ApplyTimeBasedAcSetting(StateChange e, bool allowFanAssistEnable = true)
    {
        Logger.LogDebug(
            "ApplyTimeBasedAcSetting triggered by sensor: {EntityId}, NewState: {State}",
            e.New?.EntityId,
            e.New?.State
        );

        ApplyScheduledAcSettings(allowFanAssistEnable);
    }

    private void ApplyPowerSavingModeFromWeather(StateChange e)
    {
        var weatherThresholds = climateAutomationScheduler.GetCurrentSettings().WeatherPowerSaving;

        var (_, uvIndex) = e.GetAttributeChange<double?>("uv_index");

        var (_, outdoorTemperature) = e.GetAttributeChange<double?>("temperature");

        if (!(uvIndex.HasValue && outdoorTemperature.HasValue))
        {
            Logger.LogDebug(
                "Skipping power-saving weather check: missing weather data (UvIndex={UvIndex}, OutdoorTemp={OutdoorTemp})",
                uvIndex,
                outdoorTemperature
            );

            return;
        }

        var shouldEnablePowerSaving =
            uvIndex.Value >= weatherThresholds.TriggerUvIndex
            || outdoorTemperature.Value >= weatherThresholds.TriggerOutdoorTempC;

        var shouldDisablePowerSaving =
            uvIndex.Value <= weatherThresholds.RecoveryUvIndex
            && outdoorTemperature.Value <= weatherThresholds.RecoveryOutdoorTempC;

        var toggleReason = uvIndex.Value >= weatherThresholds.TriggerUvIndex ? "uv" : "temperature";

        Logger.LogDebug(
            "Weather power-saving evaluation: UvIndex={UvIndex}, OutdoorTemp={OutdoorTemp}, ModeIsOn={ModeIsOn}, ShouldEnable={ShouldEnable}, ShouldDisable={ShouldDisable}",
            uvIndex,
            outdoorTemperature,
            _powerSavingMode.IsOn(),
            shouldEnablePowerSaving,
            shouldDisablePowerSaving
        );

        if (_powerSavingMode.IsOff() && shouldEnablePowerSaving)
        {
            _powerSavingMode.TurnOn();

            Logger.LogInformation(
                "Enabled power-saving mode from weather (Reason={Reason}, UvIndex={UvIndex}, OutdoorTemp={OutdoorTemp})",
                toggleReason,
                uvIndex,
                outdoorTemperature
            );

            return;
        }

        if (_powerSavingMode.IsOn() && shouldDisablePowerSaving)
        {
            _powerSavingMode.TurnOff();

            Logger.LogInformation(
                "Disabled power-saving mode from weather (UvIndex={UvIndex}, OutdoorTemp={OutdoorTemp})",
                uvIndex,
                outdoorTemperature
            );

            return;
        }

        Logger.LogDebug(
            "No power-saving toggle from weather (UvIndex={UvIndex}, OutdoorTemp={OutdoorTemp})",
            uvIndex,
            outdoorTemperature
        );
    }

    private void HandleBedroomSettingsChanged(ClimateSettings _)
    {
        if (MasterSwitch.IsOff())
        {
            Logger.LogDebug("Climate settings changed but master switch is off.");

            return;
        }

        Logger.LogInformation("Climate settings changed, reapplying climate settings.");

        ApplyScheduledAcSettings();
    }

    private IEnumerable<IDisposable> GetHousePresenceAutomations()
    {
        var automationSettings = climateAutomationScheduler.GetCurrentSettings().Automation;
        var houseOccupancy = entities.HouseMotionSensor;

        yield return houseOccupancy
            .OnCleared(new(Minutes: automationSettings.HouseVacantTurnOffMinutes))
            .Subscribe(_ => _ac.TurnOff());

        yield return houseOccupancy
            .OnOccupied()
            .Subscribe(e =>
            {
                var last = e.Old?.LastChanged;

                var current = e.New?.LastChanged;

                if (!(last.HasValue && current.HasValue))
                {
                    return;
                }

                var timeThresholdMinutes = automationSettings.HouseReturnMinVacantMinutes;

                var durationEmptyMinutes = (current.Value - last.Value).TotalMinutes;

                if (durationEmptyMinutes < timeThresholdMinutes)
                {
                    Logger.LogDebug(
                        "House was only empty for {Minutes} minutes. Skipping AC change.",
                        durationEmptyMinutes
                    );

                    return;
                }

                Logger.LogDebug("House was empty for {Minutes} minutes", durationEmptyMinutes);

                _ac.TurnOn();

                ApplyTimeBasedAcSetting(e);
            });
    }

    private IEnumerable<IDisposable> GetFanModeToggleAutomation()
    {
        yield return entities
            .AcFanModeToggle.OnPressed()
            .Subscribe(_ =>
            {
                var modes = new[]
                {
                    HaEntityStates.AUTO,
                    HaEntityStates.LOW,
                    HaEntityStates.MEDIUM,
                    HaEntityStates.HIGH,
                };

                var current = _ac.Attributes?.FanMode;

                var index = Array.IndexOf(modes, current);

                var next = modes[(index + 1) % modes.Length];

                _ac.SetFanMode(next);
            });
    }

    private void ApplyScheduledAcSettings(bool allowFanAssistEnable = true)
    {
        if (!climateAutomationScheduler.TryGetCurrentSetting(out var timeBlock, out var setting))
        {
            Logger.LogDebug("Skipping AC settings: No active time block");

            return;
        }

        Logger.LogDebug(
            "AC settings evaluation: TimeBlock={TimeBlock}, AC.IsOn={AcOn}",
            timeBlock,
            _ac.IsOn()
        );

        if (!_ac.IsOn())
        {
            ApplyFanAssist(targetTemp: 0, allowFanAssistEnable: false);

            Logger.LogDebug("Skipping AC settings: AC is currently OFF");

            return;
        }

        int targetTemp = climateAutomationScheduler.CalculateTemperature(
            setting,
            _motionSensor.IsOccupied(),
            _doorSensor.IsOpen()
        );

        ApplyFanAssist(targetTemp, allowFanAssistEnable);

        var currentTemp = _ac.Attributes?.Temperature;

        var currentMode = _ac.State;

        if (currentTemp == targetTemp && _ac.Is(setting.Mode))
        {
            Logger.LogDebug(
                "Skipping AC settings: Already configured correctly - Temp: {CurrentTemp}°C = {TargetTemp}°C, Mode: {CurrentMode} = {TargetMode}",
                currentTemp,
                targetTemp,
                currentMode,
                setting.Mode
            );

            return;
        }

        Logger.LogDebug(
            "Applying AC schedule for {TimeBlock}: {CurrentTemp}°C → {TargetTemp}°C, {CurrentMode} → {TargetMode}, AllowFanAssistEnable={AllowFanAssistEnable}",
            timeBlock,
            currentTemp,
            targetTemp,
            currentMode,
            setting.Mode,
            allowFanAssistEnable
        );

        _ac.SetTemperature(temperature: targetTemp, hvacMode: setting.Mode);
    }

    private void ApplyFanAssist(int targetTemp, bool allowFanAssistEnable)
    {
        var climateSettings = climateAutomationScheduler.GetCurrentSettings();

        var shouldEnableFanAssist =
            climateSettings.EnableFanAssist
            && _ac.IsOn()
            && _motionSensor.IsOccupied()
            && targetTemp >= climateSettings.FanAssistAtOrAboveSetpointC;

        if (shouldEnableFanAssist)
        {
            if (allowFanAssistEnable && _fanAutomation.IsOff())
            {
                Logger.LogDebug(
                    "Enabling fan assist: TargetTemp={TargetTemp}°C, Threshold={Threshold}°C",
                    targetTemp,
                    climateSettings.FanAssistAtOrAboveSetpointC
                );

                _fanAutomation.TurnOn();
            }

            return;
        }

        if (_fanAutomation.IsOn())
        {
            Logger.LogDebug(
                "Disabling fan assist: TargetTemp={TargetTemp}°C, Occupied={Occupied}, AcIsOn={AcIsOn}",
                targetTemp,
                _motionSensor.IsOccupied(),
                _ac.IsOn()
            );

            _fanAutomation.TurnOff();
        }
    }
}
