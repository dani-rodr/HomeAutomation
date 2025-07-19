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
        : base(factory.Create<SwitchEntity>(deviceName, "auto_calibrate"), logger)
    {
        MotionSensor = factory.Create<BinarySensorEntity>("smart_presence");
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
        yield return MotionSensor
            .StateChanges()
            .IsOn()
            .Subscribe(_ => MotionCalibrator.LogMotionTrigger(Zones, Logger));
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
    public static void LogMotionTrigger(Ld2410ZoneData[] zones, ILogger logger)
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
