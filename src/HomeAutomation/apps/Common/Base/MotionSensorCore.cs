using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public record Ld2410ZoneData(
    NumberEntity MoveThreshold,
    NumberEntity StillThreshold,
    NumericSensorEntity MoveEnergy,
    NumericSensorEntity StillEnergy
);

public class MotionSensorCore(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    string deviceName,
    ILogger logger
)
{
    private readonly IMotionSensorRestartScheduler _scheduler = scheduler;
    private readonly ILogger _logger = logger;

    public readonly BinarySensorEntity SmartPresence = factory.Create<BinarySensorEntity>(
        deviceName,
        "smart_presence"
    );
    public readonly BinarySensorEntity Presence = factory.Create<BinarySensorEntity>(
        deviceName,
        "presence"
    );
    public readonly SwitchEntity EngineeringMode = factory.Create<SwitchEntity>(
        deviceName,
        "engineering_mode"
    );
    public readonly NumberEntity SensorDelay = factory.Create<NumberEntity>(
        deviceName,
        "still_target_delay"
    );
    public readonly ButtonEntity Restart = factory.Create<ButtonEntity>(
        deviceName,
        "restart_esp32"
    );
    public readonly ButtonEntity Clear = factory.Create<ButtonEntity>(deviceName, "manual_clear");
    public readonly IReadOnlyList<Ld2410ZoneData> Zones = InitializeZones(deviceName, factory);

    private static Ld2410ZoneData[] InitializeZones(
        string deviceName,
        ITypedEntityFactory factory
    ) =>
        [
            .. Enumerable
                .Range(0, 9)
                .Select(i => new Ld2410ZoneData(
                    factory.Create<NumberEntity>(deviceName, $"g{i}_move_threshold"),
                    factory.Create<NumberEntity>(deviceName, $"g{i}_still_threshold"),
                    factory.Create<NumericSensorEntity>(deviceName, $"g{i}_move_energy"),
                    factory.Create<NumericSensorEntity>(deviceName, $"g{i}_still_energy")
                )),
        ];

    public IEnumerable<IDisposable> GetPersistentAutomations(SwitchEntity masterSwitch)
    {
        return HandleAutoCalibrateStateChange(masterSwitch)
            .Concat(HandlePresenceRecoveryToClear())
            .Concat(HandleDailyRestart());
    }

    public IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return SmartPresence
            .OnOccupied(new(CheckImmediately: true))
            .Subscribe(_ => LogMotionTrigger(Zones));
    }

    private IEnumerable<IDisposable> HandleAutoCalibrateStateChange(SwitchEntity masterSwitch)
    {
        yield return masterSwitch
            .OnChanges()
            .Subscribe(s =>
            {
                if (s.IsOn())
                    EngineeringMode.TurnOn();
                else if (s.IsOff())
                    EngineeringMode.TurnOff();
            });

        yield return EngineeringMode
            .OnTurnedOff(new(Seconds: 1))
            .Where(_ => masterSwitch.IsOn())
            .Subscribe(_ => EngineeringMode.TurnOn());
    }

    private IEnumerable<IDisposable> HandleDailyRestart() =>
        _scheduler.GetSchedules(() =>
        {
            if (SmartPresence.IsClear())
            {
                _logger.LogInformation(
                    "Scheduled restart: motion sensor is clear, pressing restart button."
                );
                Restart.Press();
            }
            else
            {
                _logger.LogInformation(
                    "Scheduled restart: motion is active, waiting for it to clear."
                );
                SmartPresence
                    .OnCleared()
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        _logger.LogInformation(
                            "Motion sensor is now clear, pressing restart button."
                        );
                        Restart.Press();
                    });
            }
        });

    private IEnumerable<IDisposable> HandlePresenceRecoveryToClear()
    {
        yield return SmartPresence
            .StateChanges()
            .Where(e => e.Old.IsUnavailable() && e.New.IsAvailable() && Presence.IsClear())
            .Subscribe(_ =>
            {
                _logger.LogInformation(
                    "SmartPresence recovered from unavailable to clear. Triggering Clear.Press()."
                );
                Clear.Press();
            });
    }

    private void LogMotionTrigger(IReadOnlyList<Ld2410ZoneData> zones)
    {
        foreach (var zone in zones)
        {
            if (zone.MoveEnergy.State <= zone.MoveThreshold.State)
                continue;

            _logger.LogInformation(
                "{MoveEnergyId}: MoveEnergy={MoveEnergy}, {MoveThresholdId}: MoveThreshold={MoveThreshold}",
                zone.MoveEnergy.EntityId,
                zone.MoveEnergy.State,
                zone.MoveThreshold.EntityId,
                zone.MoveThreshold.State
            );
        }
    }
}
