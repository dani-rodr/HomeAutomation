namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop : ComputerBase
{
    protected override string ShowEvent { get; } = "show_pc";
    protected override string HideEvent { get; } = "hide_pc";
    private readonly BinarySensorEntity powerPlugThreshold;
    private readonly BinarySensorEntity networkStatus;
    private readonly SwitchEntity powerSwitch;
    private const string MOONLIGHT_APP = "com.limelight";

    public Desktop(
        IDesktopEntities entities,
        IEventHandler eventHandler,
        INotificationServices notificationServices,
        ILogger logger
    )
        : base(eventHandler, logger)
    {
        powerPlugThreshold = entities.PowerPlugThreshold;
        networkStatus = entities.NetworkStatus;
        powerSwitch = entities.PowerSwitch;
        Automations.Add(LaunchMoonlightApp(entities.RemotePcButton, notificationServices));
    }

    public override bool IsOn() => GetPowerState(powerPlugThreshold.State, networkStatus.State);

    public override IObservable<bool> StateChanges() =>
        Observable
            .CombineLatest(
                powerPlugThreshold.StateChanges(),
                networkStatus.StateChanges(),
                (powerState, networkState) => GetPowerState(powerState.New?.State, networkState.New?.State)
            )
            .StartWith(GetPowerState(powerPlugThreshold.State, networkStatus.State))
            .DistinctUntilChanged();

    private static IDisposable LaunchMoonlightApp(
        InputButtonEntity button,
        INotificationServices notificationServices
    ) =>
        button
            .StateChanges()
            .IsValidButtonPress()
            .Subscribe(e =>
            {
                if (e.UserId() == HaIdentity.DANIEL_RODRIGUEZ)
                {
                    notificationServices.LaunchAppPocoF4(MOONLIGHT_APP);
                    return;
                }
                if (e.UserId() == HaIdentity.MIPAD5)
                {
                    notificationServices.LaunchAppMiPad(MOONLIGHT_APP);
                    return;
                }
            });

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
