namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomation(
    ILgDisplay monitor,
    IComputer desktop,
    IComputer laptop,
    IEventHandler eventHandler,
    SwitchEntity masterSwitch,
    ILogger<DisplayAutomation> logger
) : ToggleableAutomation(masterSwitch, logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [
            HandleNfcScan(),
            .. ObserveComputer(desktop, ShowPcDisplay, HandlePcTurnedOff),
            .. ObserveComputer(laptop, ShowLaptopDisplay, HandleLaptopTurnedOff),
        ];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable HandleNfcScan() =>
        eventHandler.OnNfcScan(NFC_ID.DESK).Subscribe(ToggleDisplay);

    private void ToggleDisplay(string tagId)
    {
        if (monitor.IsShowingPc && laptop.IsOn())
        {
            ShowLaptopDisplay();
            return;
        }
        if (monitor.IsShowingLaptop && desktop.IsOn())
        {
            ShowPcDisplay();
            return;
        }
        ShowLaptopDisplay();
    }

    private static IEnumerable<IDisposable> ObserveComputer(
        IComputer device,
        Action onTurnedOn,
        Action onTurnedOff
    )
    {
        yield return device
            .StateChanges()
            .DistinctUntilChanged()
            .Subscribe(isOn =>
            {
                if (isOn)
                {
                    onTurnedOn();
                }
                else
                {
                    onTurnedOff();
                }
            });
        yield return device.OnShowRequested().Subscribe(_ => onTurnedOn());
        yield return device
            .OnHideRequested()
            .Subscribe(_ =>
            {
                onTurnedOff();
                device.TurnOff();
            });
    }

    private void ShowPcDisplay()
    {
        Logger.LogDebug(
            "Switching display to PC. Desktop: {Desktop}, Laptop: {Laptop}",
            desktop.IsOn(),
            laptop.IsOn()
        );
        monitor.ShowPC();
    }

    private void ShowLaptopDisplay()
    {
        Logger.LogDebug(
            "Switching display to Laptop. Desktop: {Desktop}, Laptop: {Laptop}",
            desktop.IsOn(),
            laptop.IsOn()
        );
        if (!laptop.IsOn())
        {
            laptop.TurnOn();
        }
        monitor.ShowLaptop();
    }

    private void TurnOffDisplay()
    {
        Logger.LogDebug(
            "Turning off display. Desktop: {Desktop}, Laptop: {Laptop}",
            desktop.IsOn(),
            laptop.IsOn()
        );
        monitor.TurnOff();
    }

    private void HandlePcTurnedOff()
    {
        if (laptop.IsOn())
        {
            ShowLaptopDisplay();
        }
        else
        {
            TurnOffDisplay();
        }
    }

    private void HandleLaptopTurnedOff()
    {
        if (desktop.IsOn())
        {
            ShowPcDisplay();
        }
        else
        {
            TurnOffDisplay();
        }
    }
}
