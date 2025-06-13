namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(IDesktopEntities entities, IEventHandler eventHandler, ILogger logger)
    : ComputerBase(eventHandler, logger)
{
    protected override string ShowEvent { get; } = "show_pc";
    protected override string HideEvent { get; } = "hide_pc";
    private readonly BinarySensorEntity powerPlugThreshold = entities.PowerPlugThreshold;
    private readonly BinarySensorEntity networkStatus = entities.NetworkStatus;
    private readonly SwitchEntity powerSwitch = entities.PowerSwitch;

    public override bool IsOn() => GetPowerState(powerPlugThreshold.State, networkStatus.State);

    public override IObservable<bool> StateChanges()
    {
        return Observable
            .CombineLatest(
                powerPlugThreshold.StateChanges().Select(s => s.New?.State),
                networkStatus.StateChanges().Select(s => s.New?.State),
                GetPowerState
            )
            .StartWith(GetPowerState(powerPlugThreshold.State, networkStatus.State));
    }

    private static bool GetPowerState(string? powerState, string? netState)
    {
        if (netState.IsDisconnected())
        {
            return false;
        }
        return powerState.IsOn() || netState.IsConnected();
    }

    public override void TurnOff() => powerSwitch.TurnOff();

    public override void TurnOn() => powerSwitch.TurnOn();
}
