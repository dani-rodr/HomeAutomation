using System;
using System.Reactive;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.EventHandlers;

namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(
    IDisplayAutomationEntities entities,
    LgDisplay monitor,
    Desktop desktop,
    Laptop laptop,
    ILogger logger,
    IEventHandler eventHandler
) : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [
            GetScreenBrightnessAutomation(),
            GetScreenStateAutomation(),
            GetNfcAutomation(),
            .. GetPcAutomations(),
            .. GetLaptopAutomations(),
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable GetNfcAutomation() => eventHandler.OnNfcScan(NFC_ID.DESK).Subscribe(ToggleMonitor);

    private void ToggleMonitor(string tagId)
    {
        if (desktop.IsOn() && monitor.IsShowingPc)
        {
            laptop.TurnOn();
            ShowLaptop(true);
        }
        else if (desktop.IsOn())
        {
            ShowPc(true);
        }
        else
        {
            ShowLaptop(true);
        }
    }

    private static IEnumerable<IDisposable> GetComputerAutomations(
        ComputerBase device,
        Action<bool> onShow,
        Action<bool> onHide
    )
    {
        yield return device.StateChanges().Subscribe(onShow);
        yield return device.OnShowRequested().Subscribe(onShow);
        yield return device.OnHideRequested().Subscribe(onHide);
    }

    private IEnumerable<IDisposable> GetPcAutomations() => GetComputerAutomations(desktop, ShowPc, HidePc);

    private IEnumerable<IDisposable> GetLaptopAutomations() => GetComputerAutomations(laptop, ShowLaptop, HideLaptop);

    private void ShowPc(bool isOn) => UpdateDisplay(isOn, monitor.ShowPC, laptop.IsOn, monitor.ShowLaptop);

    private void HidePc(bool _) => UpdateDisplay(false, monitor.ShowPC, laptop.IsOn, monitor.ShowLaptop);

    private void ShowLaptop(bool isOn) => UpdateDisplay(isOn, monitor.ShowLaptop, desktop.IsOn, monitor.ShowPC);

    private void HideLaptop(bool _)
    {
        UpdateDisplay(false, monitor.ShowLaptop, desktop.IsOn, monitor.ShowPC);
        laptop.TurnOff();
    }

    private void UpdateDisplay(bool isPrimaryOn, Action showPrimary, Func<bool> isFallbackOn, Action showFallback)
    {
        Logger.LogDebug(
            "UpdateDisplay triggered. isPrimaryOn: {Primary}, isFallbackOn: {Fallback}, Desktop: {Desktop}, Laptop: {Laptop}",
            isPrimaryOn,
            isFallbackOn(),
            desktop.IsOn(),
            laptop.IsOn()
        );
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
