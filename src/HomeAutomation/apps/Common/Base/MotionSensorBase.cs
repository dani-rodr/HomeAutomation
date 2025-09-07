using System.Linq;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class MotionSensorBase(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    string deviceName,
    ILogger logger
) : ToggleableAutomation(factory.Create<SwitchEntity>(deviceName, "auto_calibrate"), logger)
{
    private readonly BinarySensorEntity SmartPresence = factory.Create<BinarySensorEntity>(
        deviceName,
        "smart_presence"
    );
    private readonly BinarySensorEntity Presence = factory.Create<BinarySensorEntity>(
        deviceName,
        "presence"
    );
    private readonly SwitchEntity EngineeringMode = factory.Create<SwitchEntity>(
        deviceName,
        "engineering_mode"
    );
    private readonly NumberEntity SensorDelay = factory.Create<NumberEntity>(
        deviceName,
        "still_target_delay"
    );
    private readonly ButtonEntity Restart = factory.Create<ButtonEntity>(
        deviceName,
        "restart_esp32"
    );
    private readonly ButtonEntity Clear = factory.Create<ButtonEntity>(deviceName, "manual_clear");
    private readonly IReadOnlyList<Ld2410ZoneData> Zones =
    [
        .. InitializeZoneEntities(deviceName, factory),
    ];

    // TODO: remove motion related entities from other class
    // use this class instead
    // implement sensor delay logic here

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [
            .. HandleAutoCalibrateStateChange(),
            .. HandlePresenceRecoveryToClear(),
            .. HandleDailyRestart(),
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return SmartPresence
            .OnOccupied()
            .Subscribe(_ => MotionCalibrator.LogMotionTrigger(Zones, Logger));
    }

    private static IEnumerable<Ld2410ZoneData> InitializeZoneEntities(
        string deviceName,
        ITypedEntityFactory factory
    ) =>
        Enumerable
            .Range(0, 9)
            .Select(i => new Ld2410ZoneData(
                factory.Create<NumberEntity>(deviceName, $"g{i}_move_threshold"),
                factory.Create<NumberEntity>(deviceName, $"g{i}_still_threshold"),
                factory.Create<NumericSensorEntity>(deviceName, $"g{i}_move_energy"),
                factory.Create<NumericSensorEntity>(deviceName, $"g{i}_still_energy")
            ));

    private IEnumerable<IDisposable> HandleAutoCalibrateStateChange()
    {
        yield return MasterSwitch
            .OnChanges()
            .Subscribe(s =>
            {
                if (s.IsOn())
                {
                    EngineeringMode.TurnOn();
                }
                else if (s.IsOff())
                {
                    EngineeringMode.TurnOff();
                }
            });
        yield return EngineeringMode
            .OnTurnedOff(new(Seconds: 1))
            .Where(_ => MasterSwitch.IsOn())
            .Subscribe(_ => EngineeringMode.TurnOn());
    }

    private IEnumerable<IDisposable> HandleDailyRestart() =>
        scheduler.GetSchedules(() =>
        {
            if (SmartPresence.IsClear())
            {
                Logger.LogInformation(
                    "Scheduled restart: motion sensor is clear, pressing restart button."
                );
                Restart.Press();
            }
            else
            {
                Logger.LogInformation(
                    "Scheduled restart: motion is active, waiting for it to clear."
                );
                SmartPresence
                    .OnCleared()
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        Logger.LogInformation(
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
            .Where(e => e.Old.IsUnavailable() && e.New.IsAvailable() && Presence.IsClear() == true)
            .Subscribe(_ =>
            {
                Logger.LogInformation(
                    "SmartPresence recovered from unavailable to clear. Triggering Clear.Press()."
                );
                Clear.Press();
            });
    }
}

public static class MotionCalibrator
{
    public static void LogMotionTrigger(IReadOnlyList<Ld2410ZoneData> zones, ILogger logger)
    {
        foreach (var zone in zones)
        {
            if (zone.MoveEnergy.State <= zone.MoveThreshold.State)
            {
                continue;
            }
            logger.LogInformation(
                "{MoveEnergyId}: MoveEnergy={MoveEnergy}, {MoveThresholdId}: MoveThreshold={MoveThreshold}",
                zone.MoveEnergy.EntityId,
                zone.MoveEnergy.State,
                zone.MoveThreshold.EntityId,
                zone.MoveThreshold.State
            );
        }
    }
}

public record Ld2410ZoneData(
    NumberEntity MoveThreshold,
    NumberEntity StillThreshold,
    NumericSensorEntity MoveEnergy,
    NumericSensorEntity StillEnergy
);
