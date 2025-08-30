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
        yield return MotionSensor.StateChangesWithCurrent().IsOn().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor
            .StateChangesWithCurrent()
            .IsOff()
            .Subscribe(_ =>
            {
                Light.TurnOff();
                mirrorLight.TurnOff();
            });
        yield return entities
            .MiScalePresenceSensor.StateChanges()
            .IsOn()
            .Subscribe(_ => mirrorLight.TurnOn());
    }

    private IEnumerable<IDisposable> AutoTogglePantryMotionSensor()
    {
        yield return entities
            .BedroomDoor.StateChanges()
            .Subscribe(e =>
            {
                if (e.IsOpen())
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
        var pantryChanges = MotionSensor.StateChangesWithCurrent();
        var bathroomChanges = entities.BathroomMotionSensor.StateChangesWithCurrent();
        yield return pantryChanges
            .IsOn()
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Pantry motion detected - activating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOn();
            });
        yield return Observable
            .CombineLatest(
                pantryChanges.IsOff(),
                bathroomChanges.IsOff(),
                (pantryOff, bathOff) => true
            )
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Both sensors off ({Pantry}: {PantryState}, {Bathroom}: {BathroomState}) - starting 1-minute delay before deactivating bathroom automation {EntityId}",
                    MotionSensor.EntityId,
                    entities.BathroomMotionSensor.EntityId,
                    MotionSensor.State,
                    entities.BathroomMotionSensor.State,
                    entities.BathroomMotionAutomation.EntityId
                );
            });

        yield return Observable
            .CombineLatest(
                pantryChanges.IsOff().ForMinutes(1),
                bathroomChanges.IsOff().ForMinutes(1),
                (pantryOff, bathOff) => true
            )
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Both sensors remained off for 1 minute - deactivating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOff();
            });
    }
}
