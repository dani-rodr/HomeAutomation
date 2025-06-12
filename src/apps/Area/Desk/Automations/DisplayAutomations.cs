using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(Entities entities, LgDisplay monitor, Desktop desktop, Laptop laptop, ILogger logger)
    : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [GetScreenBrightnessAutomation(), GetScreenStateAutomation(), .. GetPcAutamations(), .. GetLaptopAutomations()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IEnumerable<IDisposable> GetPcAutamations()
    {
        yield return desktop.StateChanges().Subscribe(ShowPc);
        yield return desktop.OnShowRequested().Subscribe(ShowPc);
        yield return desktop.OnHideRequested().Subscribe(HidePc);
    }

    private IEnumerable<IDisposable> GetLaptopAutomations() => [];

    private void ShowPc(bool isOn)
    {
        if (isOn)
        {
            monitor.ShowPC();
            return;
        }
        monitor.TurnOff();
    }

    private void HidePc(bool _)
    {
        if (laptop.IsOn())
        {
            monitor.ShowLaptop();
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
