namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(
    IDesktopEntities entities,
    IEventHandler eventHandler,
    INotificationServices notificationServices,
    ILogger<Desktop> logger
) : ComputerBase(eventHandler, logger), IDesktop
{
    protected override string ShowEvent { get; } = "show_pc";
    protected override string HideEvent { get; } = "hide_pc";
    private readonly SwitchEntity power = entities.Power;
    private const string MOONLIGHT_APP = "com.limelight";

    public override bool IsOn() => power.IsOn();

    public override IObservable<bool> StateChanges() =>
        Observable.Merge(
            power.OnTurnedOn(new(CheckImmediately: false)).Select(_ => true),
            power.OnTurnedOff(new(CheckImmediately: false, Seconds: 1)).Select(_ => false)
        );

    protected override IEnumerable<IDisposable> GetAutomations() => [LaunchMoonlightApp()];

    private IDisposable LaunchMoonlightApp() =>
        entities
            .RemotePcButton.OnPressed()
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

    public override void TurnOff() => power.TurnOff();

    public override void TurnOn() => power.TurnOn();
}
