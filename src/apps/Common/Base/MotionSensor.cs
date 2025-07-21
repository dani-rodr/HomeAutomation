using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public class MotionSensor(
    ITypedEntityFactory factory,
    IMotionSensorRestartScheduler scheduler,
    string deviceName,
    ILogger logger
) : AutomationBase(factory.Create<SwitchEntity>(deviceName, "auto_calibrate"), logger)
{
    /// <summary>
    /// Provides access to the master switch for external automations
    /// </summary>
    public SwitchEntity GetMasterSwitch() => MasterSwitch;

    public readonly BinarySensorEntity MotionSensorEntity = factory.Create<BinarySensorEntity>(
        deviceName,
        "smart_presence"
    );
    private readonly SwitchEntity EngineeringMode = factory.Create<SwitchEntity>(
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
    private readonly IReadOnlyList<Ld2410ZoneData> Zones =
    [
        .. InitializeZoneEntities(deviceName, factory),
    ];

    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [
            MasterSwitch
                .StateChanges()
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
                }),
            EngineeringMode
                .StateChanges()
                .IsOffForSeconds(1)
                .Where(_ => MasterSwitch.IsOn())
                .Subscribe(_ => EngineeringMode.TurnOn()),
            .. DailyRestart(),
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensorEntity
            .StateChangesWithCurrent()
            .IsOn()
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

    private IEnumerable<IDisposable> DailyRestart() =>
        scheduler.GetSchedules(() =>
        {
            if (MotionSensorEntity.IsClear())
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
                MotionSensorEntity
                    .StateChanges()
                    .Where(e => MotionSensorEntity.IsClear())
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
