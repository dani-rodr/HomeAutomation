using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(Entities entities, LgDisplay monitor, Desktop desktop, ILogger logger)
    : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [GetScreenBrightnessAutomation(), GetScreenStateAutomation(), .. GetPcStateAutomations()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IEnumerable<IDisposable> GetPcStateAutomations()
    {
        yield return desktop.GetPowerState().Subscribe(ShowPc);
        yield return desktop.OnShowRequested().Subscribe(ShowPc);
    }

    private void ShowPc(bool isOn)
    {
        if (isOn)
        {
            monitor.ShowPC();
            return;
        }
        monitor.TurnOff();
    }

    private IDisposable GetScreenStateAutomation()
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

    private IDisposable GetScreenBrightnessAutomation()
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
