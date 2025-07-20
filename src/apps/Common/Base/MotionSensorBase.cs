using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class MotionSensorBase(
    ITypedEntityFactory factory,
    string deviceName,
    ILogger logger
) : AutomationBase(factory.Create<SwitchEntity>(deviceName, "auto_calibrate"), logger)
{
    protected readonly BinarySensorEntity MotionSensor = factory.Create<BinarySensorEntity>(
        deviceName,
        "smart_presence"
    );
    protected readonly SwitchEntity EngineeringMode = factory.Create<SwitchEntity>(
        deviceName,
        "engineering_mode"
    );
    protected readonly NumberEntity SensorDelay = factory.Create<NumberEntity>(
        deviceName,
        "still_target_delay"
    );
    protected readonly IReadOnlyList<Ld2410ZoneData> Zones =
    [
        .. InitializeZoneEntities(deviceName, factory),
    ];

    // TODO: remove motion related entities from other class
    // use this class instead
    // implement sensor delay logic here

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
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        yield return MotionSensor
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
