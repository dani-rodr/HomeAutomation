namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(
    IDesktopEntities entities,
    IEventHandler eventHandler,
    INotificationServices notificationServices,
    IScheduler scheduler,
    ILogger<Desktop> logger
) : ComputerBase(eventHandler, logger), IDesktop
{
    protected override string ShowEvent { get; } = "show_pc";
    protected override string HideEvent { get; } = "hide_pc";
    private readonly SwitchEntity power = entities.Power;
    private const string MOONLIGHT_APP = "com.limelight";

    public override bool IsOn() => power.IsOn();

    public override IObservable<bool> StateChanges() =>
        power
            .StateChanges()
            .Select(s => s.IsOn())
            .DistinctUntilChanged()
            .Select(isOn =>
            {
                if (!isOn)
                {
                    return Observable.Return(false).Delay(TimeSpan.FromSeconds(1), scheduler);
                }
                else
                {
                    return Observable.Return(true);
                }
            })
            .Switch();

    protected override IEnumerable<IDisposable> GetAutomations() => [LaunchMoonlightApp()];

    private IDisposable LaunchMoonlightApp() =>
        entities
            .RemotePcButton.StateChanges()
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

    public override void TurnOff() => power.TurnOff();

    public override void TurnOn() => power.TurnOn();
}
