namespace HomeAutomation.apps.Area.Pantry.Automations;

public class LightAutomation(IPantryLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override int SensorWaitTime => 5;
    protected override int SensorActiveDelayValue => 5;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [.. AutoTogglePantryMotionSensor(), .. AutoToggleBathroomMotionSensor()];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        var mirrorLight = entities.MirrorLight;
        yield return MotionSensor
            .OnOccupied(new(CheckImmediately: true))
            .Subscribe(_ => Light.TurnOn());
        yield return MotionSensor
            .OnCleared(new(CheckImmediately: true))
            .Subscribe(_ =>
            {
                Light.TurnOff();
                mirrorLight.TurnOff();
            });
        yield return entities
            .MiScalePresenceSensor.OnOccupied()
            .Subscribe(_ => mirrorLight.TurnOn());
    }

    private IEnumerable<IDisposable> AutoTogglePantryMotionSensor()
    {
        yield return entities
            .BedroomDoor.OnChanges()
            .Subscribe(e =>
            {
                if (e.Entity.IsOpen())
                {
                    MasterSwitch.TurnOff();
                    Light.TurnOn();
                    return;
                }
                MasterSwitch.TurnOn();
            });
    }

    private IEnumerable<IDisposable> AutoToggleBathroomMotionSensor()
    {
        yield return MotionSensor
            .OnOccupied(new(CheckImmediately: true))
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Pantry motion detected - activating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOn();
            });
        var turnOffDelay = 60;
        yield return Observable
            .CombineLatest(MotionSensor.OnCleared(), entities.BathroomMotionSensor.OnCleared())
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Both sensors off ({Pantry}: {PantryState}, {Bathroom}: {BathroomState}) - starting {turnOffDelay}-second delay before deactivating bathroom automation {EntityId}",
                    MotionSensor.EntityId,
                    entities.BathroomMotionSensor.EntityId,
                    MotionSensor.State,
                    entities.BathroomMotionSensor.State,
                    turnOffDelay,
                    entities.BathroomMotionAutomation.EntityId
                );
            });
        yield return MotionSensor
            .OnCleared(new(Seconds: turnOffDelay))
            .CombineLatest(entities.BathroomMotionSensor.OnCleared(new(Seconds: turnOffDelay)))
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Both sensors remained off for {turnOffDelay} seconds - deactivating bathroom automation {EntityId}",
                    turnOffDelay,
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOff();
            });
    }
}
