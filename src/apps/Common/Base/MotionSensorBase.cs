namespace HomeAutomation.apps.Common.Base;

public abstract class MotionSensorBase : AutomationBase
{
    protected readonly BinarySensorEntity MotionSensor;
    protected readonly SwitchEntity EngineeringMode;
    protected readonly NumberEntity SensorDelay;
    protected Ld2410ZoneData[] Zones { get; } = new Ld2410ZoneData[9];

    // TODO: remove motion related entities from other class
    // use this class instead
    // implement sensor delay logic here

    public MotionSensorBase(ITypedEntityFactory factory, string deviceName, ILogger logger)
        : base(factory.Create<SwitchEntity>($"{deviceName}_auto_calibrate"), logger)
    {
        factory.DeviceName = deviceName;
        MotionSensor = factory.Create<BinarySensorEntity>("smart_presense");
        EngineeringMode = factory.Create<SwitchEntity>("engineering_mode");
        SensorDelay = factory.Create<NumberEntity>("still_target_delay");
        InitializeZoneEntities(factory);
    }

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
        return MotionCalibrator.Start(Zones, Logger);
    }

    private void InitializeZoneEntities(ITypedEntityFactory factory)
    {
        for (int i = 0; i < Zones.Length; i++)
        {
            Zones[i] = new Ld2410ZoneData(
                factory.Create<NumberEntity>($"g{i}_move_threshold"),
                factory.Create<NumberEntity>($"g{i}_still_threshold"),
                factory.Create<NumericSensorEntity>($"g{i}_move_energy"),
                factory.Create<NumericSensorEntity>($"g{i}_still_energy")
            );
        }
    }
}

public static class MotionCalibrator
{
    public static IEnumerable<IDisposable> Start(Ld2410ZoneData[] zones, ILogger logger)
    {
        logger.LogInformation("Starting motion calibration");

        foreach (var zone in zones)
        {
            yield return zone
                .MoveEnergy.StateChanges()
                .Where(s => s.Entity.State > zone.MoveThreshold.State)
                .Select(s => s.Entity.State)
                .Subscribe(s =>
                {
                    if (s.HasValue)
                    {
                        // var updatedValue = s.Value + 1;
                        // zone.MoveThreshold.SetNumericValue(updatedValue);
                        logger.LogInformation(
                            "{MoveEnergyId}: MoveEnergy={MoveEnergy}, {MoveThresholdId}: MoveThreshold={MoveThreshold}",
                            zone.MoveEnergy.EntityId,
                            zone.MoveEnergy.State,
                            zone.MoveThreshold.EntityId,
                            zone.MoveThreshold.State
                        );
                    }
                });
        }
    }
}

public record Ld2410ZoneData(
    NumberEntity MoveThreshold,
    NumberEntity StillThreshold,
    NumericSensorEntity MoveEnergy,
    NumericSensorEntity StillEnergy
);
