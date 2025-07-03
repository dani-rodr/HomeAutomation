namespace HomeAutomation.apps.Area.Pantry.Automations;

public class MotionAutomation(IPantryMotionEntities entities, ILogger<MotionAutomation> logger)
    : MotionAutomationBase(entities, logger)
{
    protected override int SensorWaitTime => 10;

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [entities.BedroomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch?.TurnOn())];

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
}
