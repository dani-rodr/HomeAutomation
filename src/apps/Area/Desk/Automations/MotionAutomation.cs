namespace HomeAutomation.apps.Area.Desk.Automations;

public class MotionAutomation(IMotionAutomationEntities entities, ILogger logger)
    : MotionAutomationBase(entities.MasterSwitch, entities.MotionSensor, entities.Light, logger, entities.SensorDelay)
{
    protected override IEnumerable<IDisposable> GetLightAutomations()
    {
        yield return MotionSensor.StateChanges().IsOn().Subscribe(e => Light.TurnOn());
        yield return MotionSensor.StateChanges().IsOff().Subscribe(e => Light.TurnOff());
    }
}
