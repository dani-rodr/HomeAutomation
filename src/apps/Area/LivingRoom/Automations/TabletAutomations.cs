using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomations(ITabletAutomationEntities entities, ILogger logger) 
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
