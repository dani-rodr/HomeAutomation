using System.Linq;

namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class ClimateAutomation(
    IClimateEntities entities,
    IClimateScheduler scheduler,
    ILogger<ClimateAutomation> logger
) : ToggleableAutomation(entities.MasterSwitch, logger)
{
    private readonly ClimateEntity _ac = entities.AirConditioner;
    private readonly BinarySensorEntity _motionSensor = entities.MotionSensor;
    private readonly BinarySensorEntity _doorSensor = entities.Door;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return scheduler.GetResetSchedule();
        yield return _ac.StateAllChanges()
            .IsManuallyOperated()
            .Subscribe(TurnOffMasterSwitchOnManualOperation);
        yield return _motionSensor
            .OnCleared(new(Hours: 1))
            .Where(_ => MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch.TurnOn());
        yield return MasterSwitch.OnTurnedOff(new(Hours: 8)).Subscribe(_ => MasterSwitch.TurnOn());
        yield return _doorSensor
            .OnClosed()
            .Where(_ => MasterSwitch.IsOn())
            .Subscribe(ApplyTimeBasedAcSetting);
        yield return MasterSwitch.OnTurnedOn().Subscribe(ApplyTimeBasedAcSetting);
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() =>
        [
            .. scheduler.GetSchedules(() =>
            {
                ApplyScheduledAcSettings(scheduler.FindCurrentTimeBlock());
            }),
            .. GetSensorBasedAutomations(),
            .. GetHousePresenceAutomations(),
            .. GetFanModeToggleAutomation(),
        ];

    private void TurnOffMasterSwitchOnManualOperation(StateChange e)
    {
        if (e.New.IsUnavailable() || e.Old.IsUnavailable())
        {
            Logger.LogDebug("AC states is unavailable, skipping master switch turn-off.");
            return;
        }
        var (oldTemp, newTemp) = e.GetAttributeChange<double?>("temperature");
        var stateChanged = e.New?.State != e.Old?.State;
        if (stateChanged || (oldTemp.HasValue && newTemp.HasValue && oldTemp != newTemp))
        {
            MasterSwitch.TurnOff();
            Logger.LogDebug(
                "AC state changed: {OldState} ➜ {NewState} | Temp: {OldTemp} ➜ {NewTemp} | By: {User}",
                e.Old?.State,
                e.New?.State,
                oldTemp?.ToString() ?? "N/A",
                newTemp?.ToString() ?? "N/A",
                e.Username() ?? "unknown"
            );
        }
    }

    private IEnumerable<IDisposable> GetSensorBasedAutomations()
    {
        yield return _doorSensor.OnOpened(new(Minutes: 5)).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.OnCleared(new(Minutes: 10)).Subscribe(ApplyTimeBasedAcSetting);
        yield return _motionSensor.OnOccupied().Subscribe(ApplyTimeBasedAcSetting);
    }

    private void ApplyTimeBasedAcSetting(StateChange e)
    {
        Logger.LogDebug(
            "ApplyTimeBasedAcSetting triggered by sensor: {EntityId}, NewState: {State}",
            e.New?.EntityId,
            e.New?.State
        );

        ApplyScheduledAcSettings(scheduler.FindCurrentTimeBlock());
    }

    private IEnumerable<IDisposable> GetHousePresenceAutomations()
    {
        var houseOccupancy = entities.HouseMotionSensor;
        yield return houseOccupancy.OnCleared(new(Minutes: 30)).Subscribe(_ => _ac.TurnOff());
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
                var timeThresholdMinutes = 20;

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

    private void ApplyScheduledAcSettings(TimeBlock? timeBlock)
    {
        Logger.LogDebug(
            "AC settings evaluation: TimeBlock={TimeBlock}, AC.IsOn={AcOn}",
            timeBlock?.ToString() ?? "None",
            _ac.IsOn()
        );

        if (timeBlock is null)
        {
            Logger.LogDebug("Skipping AC settings: No active time block");
            return;
        }

        if (!scheduler.TryGetSetting(timeBlock.Value, out var setting))
        {
            Logger.LogDebug(
                "Skipping AC settings: No settings found for time block {TimeBlock}",
                timeBlock.Value
            );
            return;
        }

        if (!_ac.IsOn())
        {
            Logger.LogDebug("Skipping AC settings: AC is currently OFF");
            return;
        }

        int targetTemp = scheduler.CalculateTemperature(
            setting,
            _motionSensor.IsOccupied(),
            _doorSensor.IsOpen()
        );
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
            "Applying AC schedule for {TimeBlock}: {CurrentTemp}°C → {TargetTemp}°C, {CurrentMode} → {TargetMode}, ActivateFan={ActivateFan}",
            timeBlock.Value,
            currentTemp,
            targetTemp,
            currentMode,
            setting.Mode,
            setting.ActivateFan
        );
        _ac.SetTemperature(temperature: targetTemp, hvacMode: setting.Mode);
    }
}
