namespace HomeAutomation.apps.Area.Pantry.Automations;

public class LightAutomation(IPantryLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override int SensorWaitTime => 5;
    protected override int SensorActiveDelayValue => 5;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [
            .. AutoTogglePantryMotionSensor(),
            .. AutoToggleBathroomMotionSensor(),
            .. GetMirrorLightAutomations(),
        ];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.OnOccupied().Subscribe(_ => Light.TurnOn());
        yield return MotionSensor
            .OnCleared()
            .Subscribe(_ =>
            {
                Light.TurnOff();
            });
    }

    private IEnumerable<IDisposable> GetMirrorLightAutomations() =>
        [
            entities
                .MiScalePresenceSensor.OnOccupied()
                .Where(_ => MotionSensor.IsOccupied())
                .Subscribe(_ => entities.MirrorLight.TurnOn()),
            MotionSensor
                .OnCleared()
                .Subscribe(_ =>
                {
                    entities.MirrorLight.TurnOff();
                }),
        ];

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
            .OnOccupied()
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Pantry motion detected - activating bathroom automation {EntityId}",
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOn();
            });
        yield return DeactivateWhenBothSensorsClear(MotionSensor, entities.BathroomMotionSensor);

        yield return DeactivateWhenBothSensorsClear(entities.BathroomMotionSensor, MotionSensor);
    }

    IDisposable DeactivateWhenBothSensorsClear(
        BinarySensorEntity triggerSensor,
        BinarySensorEntity otherSensor,
        int turnOffDelay = 60
    ) =>
        triggerSensor
            .OnCleared(new(Seconds: turnOffDelay))
            .Where(_ => otherSensor.IsClear())
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "{Trigger} remained off for {turnOffDelay} seconds and {Other} is also off - deactivating bathroom automation {EntityId}",
                    triggerSensor.EntityId,
                    turnOffDelay,
                    otherSensor.EntityId,
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOff();
            });
}
