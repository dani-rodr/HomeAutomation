namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomations(
    Entities entities,
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    ILogger logger
) : MotionAutomationBase(masterSwitch, motionSensor, entities.Light.MipadScreen, logger)
{
    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() => [];

    private BinarySensorEntity _tabletActive = entities.BinarySensor.Mipad;

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
