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
        [GetLightMotionAutomation(), .. GetSalaLightsAutomation()];

    protected override IEnumerable<IDisposable> GetAdditionalPersistentAutomations()
    {
        yield return MasterSwitch!
            .StateChanges()
            .DistinctUntilChanged()
            .Where(_ => Light.IsOn() && monitor.IsOn())
            .Subscribe(e =>
            {
                monitor.ShowToast("LG Display Automation is {0}", e.IsOn() ? "ON" : "OFF");
            });
    }

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

    private IEnumerable<IDisposable> GetSalaLightsAutomation()
    {
        var salaLights = entities.SalaLights;
        yield return monitor.ScreenChanges.Subscribe(async screenOn =>
        {
            if (!screenOn)
            {
                return;
            }
            if (entities.SalaLights.IsOn())
            {
                await monitor.SetBrightnessHighAsync();
            }
            else
            {
                await monitor.SetBrightnessLowAsync();
            }
        });
        yield return salaLights
            .StateChanges()
            .IsOnForSeconds(1)
            .Subscribe(async _ => await monitor.SetBrightnessHighAsync());
        yield return salaLights
            .StateChanges()
            .IsOff()
            .Subscribe(async _ => await monitor.SetBrightnessLowAsync());
    }
}
