namespace HomeAutomation.apps.Area.Pantry.Automations;

public class LightAutomation(
    IPantryLightEntities entities,
    MotionAutomationBase motionAutomation,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations() =>
        [entities.BedroomDoor.StateChanges().IsOff().Subscribe(_ => MasterSwitch.TurnOn())];

    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        var mirrorLight = entities.MirrorLight;
        yield return MotionAutomation
            .GetMotionSensor()
            .StateChangesWithCurrent()
            .IsOn()
            .Subscribe(_ => Light.TurnOn());
        yield return MotionAutomation
            .GetMotionSensor()
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
