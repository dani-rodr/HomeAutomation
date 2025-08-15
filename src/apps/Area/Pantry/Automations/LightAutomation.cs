namespace HomeAutomation.apps.Area.Pantry.Automations;

public class LightAutomation(IPantryLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override int SensorWaitTime => 5;
    protected override int SensorActiveDelayValue => 5;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [
            entities.BedroomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch.TurnOn()),
            .. AutoToggleBathroomMotionSensor(),
        ];

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

    private IEnumerable<IDisposable> AutoToggleBathroomMotionSensor()
    {
        yield return MotionSensor
            .StateChangesWithCurrent()
            .IsOn()
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Pantry motion detected - activating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOn();
            });
        yield return MotionSensor
            .StateChangesWithCurrent()
            .CombineLatest(entities.BathroomMotionSensor.StateChangesWithCurrent())
            .Where(states => states.First.IsOff() && states.Second.IsOff())
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

        yield return MotionSensor
            .StateChangesWithCurrent()
            .IsOffForMinutes(1)
            .CombineLatest(
                entities.BathroomMotionSensor.StateChangesWithCurrent().IsOffForMinutes(1)
            )
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "1-minute delay completed - deactivating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOff();
            });
    }
}
