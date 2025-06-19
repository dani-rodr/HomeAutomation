namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop : ComputerBase
{
    protected override string ShowEvent { get; } = "show_pc";
    protected override string HideEvent { get; } = "hide_pc";
    private readonly SwitchEntity power;
    private const string MOONLIGHT_APP = "com.limelight";

    public Desktop(
        IDesktopEntities entities,
        IEventHandler eventHandler,
        INotificationServices notificationServices,
        ILogger logger
    )
        : base(eventHandler, logger)
    {
        power = entities.Power;
        Automations.Add(LaunchMoonlightApp(entities.RemotePcButton, notificationServices));
    }

    public override bool IsOn() => power.IsOn();

    public override IObservable<bool> StateChanges() => power.StateChanges().Select(s => s.IsOn());

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

    public override void TurnOff() => power.TurnOff();

    public override void TurnOn() => power.TurnOn();
}
