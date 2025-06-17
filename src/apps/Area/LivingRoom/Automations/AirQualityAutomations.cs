namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityAutomations(IAirQualityEntities entities, ILogger logger)
    : FanAutomationBase(
        entities.MasterSwitch,
        entities.MotionSensor,
        logger,
        entities.AirPurifierFan,
        entities.SupportingFan
    )
{
    private readonly NumericSensorEntity _airQuality = entities.Pm25Sensor;
    private readonly SwitchEntity _ledStatus = entities.LedStatus;
    protected override bool ShouldActivateFan { get; set; } = false;
    private bool IsCleaningAir { get; set; } = false;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return entities
            .MotionSensor.StateChanges()
            .IsOffForMinutes(15)
            .Where(_ => entities.MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
        yield return entities
            .AirPurifierFan.StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    MasterSwitch?.TurnOn();
                }
                else if (e.IsOff())
                {
                    MasterSwitch?.TurnOff();
                }
            });
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        int waitTime = 10;
        int cleanAirThreshold = 7;
        int dirtyAirThreshold = 75;
        var supportingFan = entities.SupportingFan;
        yield return Fan.StateChanges().Subscribe(SyncLedWithFan);
        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State <= cleanAirThreshold, waitTime)
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Air quality EXCELLENT (value: {Value} ≤ threshold: {Threshold}). IsCleaningAir={IsCleaningAir}, ShouldActivateFan={ShouldActivateFan}. Turning off main fan.",
                    e.New?.State,
                    cleanAirThreshold,
                    IsCleaningAir,
                    ShouldActivateFan
                );
                Fan.TurnOff();
            });

        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > cleanAirThreshold && s?.State <= dirtyAirThreshold, waitTime)
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Air quality MODERATE (value: {Value}, range: {MinThreshold}-{MaxThreshold}). IsCleaningAir={IsCleaningAir}, ShouldActivateFan={ShouldActivateFan}",
                    e.New?.State,
                    cleanAirThreshold,
                    dirtyAirThreshold,
                    IsCleaningAir,
                    ShouldActivateFan
                );

                Fan.TurnOn();
                if (IsCleaningAir && !ShouldActivateFan)
                {
                    Logger.LogDebug(
                        "Transitioning from cleaning mode: Supporting fan OFF, main fan ON. IsCleaningAir: {PreviousState} → false",
                        IsCleaningAir
                    );

                    supportingFan.TurnOff();
                    IsCleaningAir = false;
                    ShouldActivateFan = false;
                }
                else
                {
                    Logger.LogDebug(
                        "No transition needed: IsCleaningAir={IsCleaningAir}, ShouldActivateFan={ShouldActivateFan}",
                        IsCleaningAir,
                        ShouldActivateFan
                    );
                }
            });
        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > dirtyAirThreshold, waitTime)
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Air quality POOR (value: {Value} > threshold: {Threshold}). ShouldActivateFan={ShouldActivateFan}, IsCleaningAir={IsCleaningAir}",
                    e.New?.State,
                    dirtyAirThreshold,
                    ShouldActivateFan,
                    IsCleaningAir
                );

                if (!ShouldActivateFan)
                {
                    Logger.LogDebug(
                        "Activating supporting fan for poor air quality. IsCleaningAir: {PreviousState} → true",
                        IsCleaningAir
                    );
                    supportingFan.TurnOn();
                    IsCleaningAir = true;
                    ShouldActivateFan = false;
                }
                else
                {
                    Logger.LogDebug("Supporting fan not activated: ShouldActivateFan=true (manual override active)");
                }
            });

        yield return supportingFan
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                Logger.LogDebug(
                    "Supporting fan manually operated: {OldState} → {NewState} by {UserId}. Setting ShouldActivateFan=true",
                    e.Old?.State,
                    e.New?.State,
                    e.UserId() ?? "unknown"
                );
                ShouldActivateFan = true;
            });
        yield return supportingFan
            .StateChanges()
            .IsOffForMinutes(10)
            .Subscribe(e =>
            {
                Logger.LogDebug("Supporting fan OFF for 10+ minutes. Resetting ShouldActivateFan: true → false");
                ShouldActivateFan = false;
            });
    }

    private void SyncLedWithFan(StateChange e)
    {
        if (Fan.IsOn())
        {
            _ledStatus.TurnOn();
        }
        else
        {
            _ledStatus.TurnOff();
        }
    }
}
