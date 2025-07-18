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
        : base(factory.Create<SwitchEntity>("auto_calibrate"), logger)
    {
        factory.DeviceName = deviceName;
        MotionSensor = factory.Create<BinarySensorEntity>("smart_presense");
        EngineeringMode = factory.Create<SwitchEntity>("engineering_mode");
        SensorDelay = factory.Create<NumberEntity>("still_target_delay");
        InitializeZoneEntities(factory);
    }

    private void InitializeZoneEntities(ITypedEntityFactory factory)
    {
        for (int i = 0; i < Zones.Length; i++)
        {
            Zones[i] = new Ld2410ZoneData(
                factory.Create<NumberEntity>($"g{i}_move_threshold"),
                factory.Create<NumberEntity>($"g{i}_still_threshold"),
                factory.Create<SensorEntity>($"g{i}_move_energy"),
                factory.Create<SensorEntity>($"g{i}_still_energy")
            );
        }
    }
}

public record Ld2410ZoneData(
    NumberEntity MoveThreshold,
    NumberEntity StillThreshold,
    SensorEntity MoveEnergy,
    SensorEntity StillEnergy
);
