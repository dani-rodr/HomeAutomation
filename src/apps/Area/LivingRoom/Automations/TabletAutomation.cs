namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomation(ITabletEntities entities, ILogger logger)
    : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.TabletScreen, logger)
{
    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() => [];

    private BinarySensorEntity _tabletActive = entities.TabletActive;

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
