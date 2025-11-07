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
        var mirrorLight = entities.MirrorLight;
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
        var turnOffDelay = 60;
        yield return MotionSensor
            .OnCleared(new(Seconds: turnOffDelay))
            .Where(_ => entities.BathroomMotionSensor.IsClear())
            .Subscribe(_ =>
            {
                Logger.LogDebug(
                    "Pantry sensor remained off for {turnOffDelay} seconds and bathroom sensor is also off - deactivating bathroom automation {EntityId}",
                    turnOffDelay,
                    entities.BathroomMotionAutomation.EntityId
                );
                entities.BathroomMotionAutomation.TurnOff();
            });
    }
}
