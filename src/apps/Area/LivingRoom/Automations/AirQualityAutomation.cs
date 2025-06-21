namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class AirQualityAutomation(IAirQualityEntities entities, ILogger logger)
    : FanAutomationBase(entities.MasterSwitch, entities.MotionSensor, logger, [.. entities.Fans])
{
    private readonly NumericSensorEntity _airQuality = entities.Pm25Sensor;
    private readonly SwitchEntity _ledStatus = entities.LedStatus;
    private SwitchEntity _supportingFan => Fans[1];
    protected override bool ShouldActivateFan { get; set; } = false;
    private bool IsCleaningAir { get; set; } = false;

    private const int WAIT_TIME = 10;
    private const int CLEAN_AIR_THRESHOLD = 7;
    private const int DIRTY_AIR_THRESHOLD = 75;

    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return entities
            .MotionSensor.StateChanges()
            .IsOffForMinutes(15)
            .Where(_ => entities.MasterSwitch.IsOff())
            .Subscribe(_ => MasterSwitch?.TurnOn());
        yield return Fan.StateChanges()
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
        yield return SubscribeToFanStateChanges();
        yield return SubscribeToExcellentAirQuality();
        yield return SubscribeToModerateAirQuality();
        yield return SubscribeToPoorAirQuality();
        yield return SubscribeToManualFanOperation();
        yield return SubscribeToSupportingFanIdle();
    }

    private IDisposable SubscribeToExcellentAirQuality() =>
        _airQuality
            .StateChangesWithCurrent()
            .WhenStateIsForSeconds(s => s?.State <= CLEAN_AIR_THRESHOLD, WAIT_TIME)
            .Subscribe(e =>
            {
                Fan.TurnOff();
            });

    private IDisposable SubscribeToModerateAirQuality() =>
        _airQuality
            .StateChangesWithCurrent()
            .WhenStateIsForSeconds(
                s => s?.State > CLEAN_AIR_THRESHOLD && s?.State <= DIRTY_AIR_THRESHOLD,
                WAIT_TIME
            )
            .Subscribe(e =>
            {
                Fan.TurnOn();

                if (IsCleaningAir && !ShouldActivateFan)
                {
                    _supportingFan.TurnOff();
                    IsCleaningAir = false;
                    ShouldActivateFan = false;
                    entities.LivingRoomSwitch.TurnOn();
                }
            });

    private IDisposable SubscribeToPoorAirQuality() =>
        _airQuality
            .StateChangesWithCurrent()
            .WhenStateIsForSeconds(s => s?.State > DIRTY_AIR_THRESHOLD, WAIT_TIME)
            .Subscribe(e =>
            {
                if (!ShouldActivateFan)
                {
                    _supportingFan.TurnOn();
                    IsCleaningAir = true;
                    ShouldActivateFan = false;
                    entities.LivingRoomSwitch.TurnOff();
                }
            });

    private IDisposable SubscribeToManualFanOperation() =>
        _supportingFan
            .StateChanges()
            .IsManuallyOperated()
            .Subscribe(e =>
            {
                ShouldActivateFan = true;
            });

    private IDisposable SubscribeToSupportingFanIdle() =>
        _supportingFan
            .StateChangesWithCurrent()
            .IsOffForMinutes(10)
            .Subscribe(_ =>
            {
                ShouldActivateFan = false;
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
