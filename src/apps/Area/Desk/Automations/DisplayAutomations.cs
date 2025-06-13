using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(
    IDisplayAutomationEntities entities,
    LgDisplay monitor,
    Desktop desktop,
    Laptop laptop,
    ILogger logger
) : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [GetScreenBrightnessAutomation(), GetScreenStateAutomation(), .. GetPcAutomations(), .. GetLaptopAutomations()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IEnumerable<IDisposable> GetPcAutomations()
    {
        yield return desktop.StateChanges().Subscribe(ShowPc);
        yield return desktop.OnShowRequested().Subscribe(ShowPc);
        yield return desktop.OnHideRequested().Subscribe(HidePc);
    }

    private IEnumerable<IDisposable> GetLaptopAutomations()
    {
        yield return laptop.StateChanges().Subscribe(ShowLaptop);
        yield return laptop.OnShowRequested().Subscribe(ShowLaptop);
        yield return laptop.OnHideRequested().Subscribe(HideLaptop);
    }

    private void ShowPc(bool isOn) => UpdateDisplay(isOn, monitor.ShowPC, laptop.IsOn, monitor.ShowLaptop);

    private void HidePc(bool _) => UpdateDisplay(false, monitor.ShowPC, laptop.IsOn, monitor.ShowLaptop);

    private void ShowLaptop(bool isOn) => UpdateDisplay(isOn, monitor.ShowLaptop, desktop.IsOn, monitor.ShowPC);

    private void HideLaptop(bool _) => UpdateDisplay(false, monitor.ShowLaptop, desktop.IsOn, monitor.ShowPC);

    private void UpdateDisplay(bool isPrimaryOn, Action showPrimary, Func<bool> isFallbackOn, Action showFallback)
    {
        if (isPrimaryOn)
        {
            showPrimary();
            return;
        }

        if (isFallbackOn())
        {
            showFallback();
            return;
        }

        monitor.TurnOff();
    }

    private IDisposable GetScreenStateAutomation()
    {
        return entities
            .LgScreen.StateChanges()
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
            .LgTvBrightness.StateChanges()
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
