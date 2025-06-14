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
                Logger.LogInformation(
                    "Air quality improved (value: {Value} ≤ threshold: {Threshold}). Turning off fan.",
                    e.New?.State,
                    cleanAirThreshold
                );
                Fan.TurnOff();
            });

        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > cleanAirThreshold && s?.State <= dirtyAirThreshold, waitTime)
            .Subscribe(e =>
            {
                Fan.TurnOn();
                if (IsCleaningAir && !ShouldActivateFan)
                {
                    Logger.LogInformation("Air quality moderate (value: {Value}). Turning on main fan.", e.New?.State);

                    supportingFan.TurnOff();
                    IsCleaningAir = false;
                    ShouldActivateFan = false;
                }
            });
        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > dirtyAirThreshold, waitTime)
            .Subscribe(e =>
            {
                if (!ShouldActivateFan)
                {
                    Logger.LogInformation(
                        "Air quality poor (value: {Value} > threshold: {Threshold}). Activating supporting fan.",
                        e.New?.State,
                        dirtyAirThreshold
                    );
                    supportingFan.TurnOn();
                    IsCleaningAir = true;
                    ShouldActivateFan = false;
                }
            });

        yield return supportingFan.StateChanges().IsManuallyOperated().Subscribe(_ => ShouldActivateFan = true);
        yield return supportingFan.StateChanges().IsOffForMinutes(10).Subscribe(_ => ShouldActivateFan = false);
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
