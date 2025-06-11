namespace HomeAutomation.apps.Area.LivingRoom.Automations;

public class TabletAutomations(
    Entities entities,
    SwitchEntity masterSwitch,
    BinarySensorEntity motionSensor,
    ILogger logger
) : MotionAutomationBase(masterSwitch, motionSensor, entities.Light.MipadScreen, logger)
{
    protected override IEnumerable<IDisposable> GetSensorDelayAutomations() => [];
}
