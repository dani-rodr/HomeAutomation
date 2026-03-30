namespace HomeAutomation.apps.Area.Desk.Automations;

public class DisplayAutomation(
    ILgDisplay monitor,
    IComputer desktop,
    SwitchEntity masterSwitch,
    ILogger<DisplayAutomation> logger
) : ToggleableAutomation(masterSwitch, logger)
{
    protected override IEnumerable<IDisposable> GetPersistentAutomations() =>
        [.. ObserveComputer(desktop, ShowPcDisplay, TurnOffDisplay)];

    protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];

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
        Logger.LogDebug("Switching display to PC. Desktop: {Desktop}", desktop.IsOn());
        monitor.ShowPC();
    }

    private void TurnOffDisplay()
    {
        Logger.LogDebug("Turning off display. Desktop: {Desktop}", desktop.IsOn());
        monitor.TurnOff();
    }
}
