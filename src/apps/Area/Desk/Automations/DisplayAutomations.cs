namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomations(
    ILgDisplay monitor,
    IComputer desktop,
    IComputer laptop,
    IEventHandler eventHandler,
    ILogger logger
) : AutomationBase(logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [GetNfcAutomation(), .. GetPcAutomations(), .. GetLaptopAutomations()];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

    private IDisposable GetNfcAutomation() => eventHandler.OnNfcScan(NFC_ID.DESK).Subscribe(ToggleMonitor);

    private void ToggleMonitor(string tagId)
    {
        if (!desktop.IsOn() || monitor.IsShowingPc)
        {
            laptop.TurnOn();
            SwitchToLaptop();
        }
        else
        {
            SwitchToPc();
        }
    }

    private static IEnumerable<IDisposable> GetComputerAutomations(
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
        yield return device.OnHideRequested().Subscribe(_ => onTurnedOff());
    }

    private IEnumerable<IDisposable> GetPcAutomations() =>
        GetComputerAutomations(desktop, OnDesktopTurnedOn, OnDesktopTurnedOff);

    private IEnumerable<IDisposable> GetLaptopAutomations() =>
        GetComputerAutomations(laptop, OnLaptopTurnedOn, OnLaptopTurnedOff);

    private void OnDesktopTurnedOn() => SwitchToPc();

    private void OnDesktopTurnedOff() => HandleDesktopOff();

    private void OnLaptopTurnedOn() => SwitchToLaptop();

    private void OnLaptopTurnedOff()
    {
        HandleLaptopOff();
        laptop.TurnOff();
    }

    private void SwitchToPc()
    {
        Logger.LogDebug("Switching display to PC. Desktop: {Desktop}, Laptop: {Laptop}", desktop.IsOn(), laptop.IsOn());
        monitor.ShowPC();
    }

    private void SwitchToLaptop()
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
        Logger.LogDebug("Turning off display. Desktop: {Desktop}, Laptop: {Laptop}", desktop.IsOn(), laptop.IsOn());
        monitor.TurnOff();
    }

    private void HandleDesktopOff()
    {
        if (laptop.IsOn())
        {
            SwitchToLaptop();
        }
        else
        {
            TurnOffDisplay();
        }
    }

    private void HandleLaptopOff()
    {
        if (desktop.IsOn())
        {
            SwitchToPc();
        }
        else
        {
            TurnOffDisplay();
        }
    }
}
