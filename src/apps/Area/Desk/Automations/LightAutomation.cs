namespace HomeAutomation.apps.Area.Desk.Automations;

public class LightAutomation(
    ILightAutomationEntities entities,
    ILgDisplay monitor,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private const double LONG_SENSOR_DELAY = 60;
    private const double SHORT_SENSOR_DELAY = 20;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [MotionSensor.StateChanges().Subscribe(HandleMotionSensor)];

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetSensorDelayAutomations()
    {
        yield return monitor
            .OnSourceChange()
            .Subscribe(source =>
            {
                var delay = monitor.IsShowingPc ? LONG_SENSOR_DELAY : SHORT_SENSOR_DELAY;
                SensorDelay?.SetNumericValue(delay);
            });
    }

    private void HandleMotionSensor(StateChange e)
    {
        return;
        if (e.IsOn())
        {
            Light.TurnOn();
        }
        else if (e.IsOff())
        {
            Light.TurnOff();
        }
    }
}
