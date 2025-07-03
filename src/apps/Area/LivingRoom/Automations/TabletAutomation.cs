namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomation(ITabletEntities entities, ILogger<TabletAutomation> logger)
    : LightAutomationBase(entities, logger)
{
    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() => [];

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [MotionSensor.StateChanges().Subscribe(ToggleLights)];

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
