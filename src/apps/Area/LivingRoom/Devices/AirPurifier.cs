using System.Collections.Generic;
using apps.Area.LivingRoom;

namespace apps.Area.LivingRoom.Devices;

public class AirPurifier(Entities entities, ILogger logger) : AutomationBase(logger, entities.Switch.CleanAir)
{
    private readonly NumericSensorEntity _airQuality = entities.Sensor.XiaomiSg753990712Cpa4Pm25DensityP34;
    private readonly SwitchEntity _airPurifierFan = entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierFanSwitch;
    private readonly SwitchEntity _standFan = entities.Switch.Sonoff10023810231;
    private readonly SwitchEntity _ledStatus = entities.Switch.XiaomiSmartAirPurifier4CompactAirPurifierLedStatus;
    private bool _isCleaningAir = false;
    private bool _isStandFanManuallyOperated = false;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        int waitTime = 10;
        int cleanAirThreshold = 5;
        int dirtyAirThreshold = 75;
        yield return _airPurifierFan.StateChanges().Subscribe(SyncLedWithFan);
        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State <= cleanAirThreshold, waitTime)
            .Subscribe(_ => _airPurifierFan.TurnOff());

        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > cleanAirThreshold && s?.State <= dirtyAirThreshold, waitTime)
            .Subscribe(_ =>
            {
                _airPurifierFan.TurnOn();
                if (_isCleaningAir && !_isStandFanManuallyOperated)
                {
                    _standFan.TurnOff();
                    _isCleaningAir = false;
                    _isStandFanManuallyOperated = false;
                }
            });
        yield return _airQuality
            .StateChanges()
            .WhenStateIsForSeconds(s => s?.State > dirtyAirThreshold, waitTime)
            .Subscribe(_ =>
            {
                if (!_isStandFanManuallyOperated)
                {
                    _standFan.TurnOn();
                    _isCleaningAir = true;
                    _isStandFanManuallyOperated = false;
                }
            });

        yield return _standFan.StateChanges().IsManuallyOperated().Subscribe(_ => _isStandFanManuallyOperated = true);
        yield return _standFan.StateChanges().IsOffForMinutes(10).Subscribe(_ => _isStandFanManuallyOperated = false);
    }

    private void SyncLedWithFan(StateChange fan)
    {
        if (fan.IsOn())
        {
            _ledStatus.TurnOn();
        }
        else
        {
            _ledStatus.TurnOff();
        }
    }
}
