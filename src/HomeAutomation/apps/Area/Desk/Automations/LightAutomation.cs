namespace HomeAutomation.apps.Area.Desk.Automations;

public class LightAutomation(
    IDeskLightEntities entities,
    ILgDisplay monitor,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, logger)
{
    private const double LONG_SENSOR_DELAY = 60;
    private const double SHORT_SENSOR_DELAY = 20;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [GetLightMotionAutomation(), GetSalaLightsAutomation()];

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

    private IDisposable GetLightMotionAutomation() =>
        MotionSensor
            .OnChanges()
            .Subscribe(e =>
            {
                if (monitor.IsOff())
                {
                    return;
                }
                if (e.IsOn())
                {
                    Light.TurnOn();
                }
                else if (e.IsOff())
                {
                    Light.TurnOff();
                }
            });

    private IDisposable GetSalaLightsAutomation() =>
        entities
            .SalaLights.OnChanges()
            .Where(_ => Light.IsOn())
            .Delay(TimeSpan.FromMilliseconds(1), SchedulerProvider.Current)
            .Subscribe(state =>
            {
                var brightness = state.IsOn() ? 230 : 125;
                Logger.LogDebug(
                    "Updating Montior Brightness to {Brightness}. based on Sala Lights.",
                    brightness
                );

                Light.TurnOn(brightness: brightness);
            });
}
