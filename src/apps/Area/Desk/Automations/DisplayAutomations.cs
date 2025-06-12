using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(Entities entities, LgDisplay monitor, Desktop desktop, ILogger logger)
    : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations()
    {
        yield return GetBrightnessAutomation();
        yield return GetScreenToggleAutomation();
        yield return ToggleMonitorOnDesktopState();
    }

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable ToggleMonitorOnDesktopState()
    {
        return desktop
            .GetPowerState()
            .Subscribe(isOn =>
            {
                if (isOn)
                {
                    monitor.ShowPC();
                    return;
                }
                monitor.TurnOff();
            });
    }

    private IDisposable GetScreenToggleAutomation()
    {
        return entities
            .Switch.LgScreen.StateChanges()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    monitor.TurnOnScreen();
                    return;
                }
                if (e.IsOff())
                {
                    monitor.TurnOffScreen();
                }
            });
    }

    private IDisposable GetBrightnessAutomation()
    {
        return entities
            .InputNumber.LgTvBrightness.StateChanges()
            .Subscribe(async e =>
            {
                var newValue = e?.New?.State;
                if (newValue is double value)
                {
                    await monitor.SetBrightnessAsync((int)value);
                }
            });
    }
}
