using System.Linq;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop : ComputerBase
{
    protected override string ShowEvent { get; } = "show_laptop";
    protected override string HideEvent { get; } = "hide_laptop";
    private readonly ILaptopEntities _entities;
    private readonly IBatteryHandler _batteryHandler;

    public Laptop(
        ILaptopEntities entities,
        ILaptopScheduler scheduler,
        IBatteryHandler batteryHandler,
        IEventHandler eventHandler,
        ILogger logger
    )
        : base(eventHandler, logger)
    {
        _entities = entities;
        _batteryHandler = batteryHandler;
        Automations.Add(GetSwitchToggleAutomations());
        AddLogoffSchedules(scheduler);
    }

    public override bool IsOn() => IsOnline(_entities.VirtualSwitch.IsOn(), _entities.Session.IsUnlocked());

    public override IObservable<bool> StateChanges()
    {
        // Observables for both switch and session state changes
        var switchOn = _entities
            .VirtualSwitch.StateChanges()
            .Select(e => e.IsOn())
            .StartWith(_entities.VirtualSwitch.State.IsOn());

        var sessionUnlocked = _entities
            .Session.StateChanges()
            .Select(e => e.New?.State.IsUnlocked() ?? false)
            .Throttle(TimeSpan.FromSeconds(1)) // Avoid rapid flapping due to session state instability
            .StartWith(_entities.Session.State.IsUnlocked());

        // Combine both streams to determine whether the laptop is considered online
        return Observable.CombineLatest(switchOn, sessionUnlocked, IsOnline).DistinctUntilChanged();
    }

    public override void TurnOn()
    {
        _entities.VirtualSwitch.TurnOn();
        _batteryHandler.HandleLaptopTurnedOn();
        foreach (var button in _entities.WakeOnLanButtons)
        {
            button.Press();
        }
    }

    public override void TurnOff()
    {
        _ = _batteryHandler.HandleLaptopTurnedOffAsync();
        _entities.VirtualSwitch.TurnOff();

        if (_entities.Session.State.IsUnlocked())
        {
            _entities.Lock.Press();
        }
    }

    private static bool IsOnline(bool switchState, bool sessionState) => switchState || sessionState;

    private IDisposable GetSwitchToggleAutomations() =>
        _entities
            .VirtualSwitch.StateChanges()
            .DistinctUntilChanged()
            .Subscribe(e =>
            {
                if (e.IsOn())
                {
                    TurnOn();
                }
                else if (e.IsOff())
                {
                    TurnOff();
                }
            });

    private void AddLogoffSchedules(ILaptopScheduler scheduler)
    {
        var logoffSchedules = scheduler.GetSchedules(() =>
        {
            if (IsOn())
            {
                Logger.LogDebug("Scheduled logoff triggered: Laptop is on, executing TurnOff.");
                TurnOff();
            }
            else
            {
                Logger.LogDebug("Scheduled logoff triggered: Laptop is not on, skipping TurnOff.");
            }
        });
        foreach (var schedule in logoffSchedules)
        {
            Automations.Add(schedule);
        }
    }
}
