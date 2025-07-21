namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomation(
    ITabletEntities entities,
    MotionAutomationBase motionAutomation,
    ILogger<TabletAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{
    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [MotionAutomation.GetMotionSensor().StateChanges().Subscribe(ToggleLights)];

    private void ToggleLights(StateChange e)
    {
        if (e.IsOn())
        {
            Light.TurnOn();
        }
        else
        {
            Light.TurnOff();
        }
    }
}
