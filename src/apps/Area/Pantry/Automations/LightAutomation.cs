namespace HomeAutomation.apps.Area.Pantry.Automations;

public class LightAutomation(IPantryLightEntities entities, ILogger<LightAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override int SensorWaitTime => 10;

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
            .Subscribe(_ => entities.BathroomMotionAutomation.TurnOn());
        yield return MotionSensor
            .StateChangesWithCurrent()
            .CombineLatest(entities.BathroomMotionSensor.StateChangesWithCurrent())
            .Where(states => states.First.IsOff() && states.Second.IsOff())
            .Subscribe(_ => entities.BathroomMotionAutomation.TurnOff());
    }
}
