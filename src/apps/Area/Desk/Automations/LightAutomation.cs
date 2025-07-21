namespace HomeAutomation.apps.Area.Desk.Automations;

public class LightAutomation(
    IDeskLightEntities entities,
    MotionAutomationBase motionAutomation,
    ILgDisplay monitor,
    ILogger<LightAutomation> logger
) : LightAutomationBase(entities, motionAutomation, logger)
{
    private const double LONG_SENSOR_DELAY = 60;
    private const double SHORT_SENSOR_DELAY = 20;

    protected override IEnumerable<IDisposable> GetLightAutomations() =>
        [GetLightMotionAutomation(), GetSalaLightsAutomation()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations()
    {
        yield return MasterSwitch!
            .StateChanges()
            .Where(_ => Light.IsOn() && monitor.IsOn())
            .Subscribe(e =>
            {
                var status = e.IsOn() ? "ON" : "OFF";
                monitor.ShowToast($"LG Display Automation is {status}");
            });
    }

    protected override IEnumerable<IDisposable> GetAdditionalSwitchableAutomations()
    {
        yield return monitor
            .OnSourceChange()
            .Subscribe(source =>
            {
                var delay = monitor.IsShowingPc ? LONG_SENSOR_DELAY : SHORT_SENSOR_DELAY;
                MotionAutomation.GetSensorDelay()?.SetNumericValue(delay);
            });
    }

    private IDisposable GetLightMotionAutomation() =>
        MotionAutomation
            .GetMotionSensor()
            .StateChanges()
            .Subscribe(e =>
            {
                if (e.Entity.State.IsUnavailable() || monitor.IsOff())
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
            .SalaLights.StateChanges()
            .Subscribe(async e =>
            {
                if (e.Entity.State.IsUnavailable())
                {
                    return;
                }
                if (e.IsOn())
                {
                    await monitor.SetBrightnessHighAsync();
                }
                else if (e.IsOff())
                {
                    await monitor.SetBrightnessLowAsync();
                }
            });
}
