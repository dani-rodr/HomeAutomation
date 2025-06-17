using System.Linq;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop : ComputerBase
{
    protected override string ShowEvent { get; } = "show_laptop";
    protected override string HideEvent { get; } = "hide_laptop";
    private readonly ILaptopEntities _entities;

    public Laptop(ILaptopEntities entities, ILaptopScheduler scheduler, IEventHandler eventHandler, ILogger logger)
        : base(eventHandler, logger)
    {
        _entities = entities;
        Automations.Add(
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
                })
        );
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
        _entities.PowerPlug.TurnOn();
        foreach (var button in _entities.WakeOnLanButtons)
        {
            button.Press();
        }
    }

    public override void TurnOff()
    {
        _entities.VirtualSwitch.TurnOff();

        if (_entities.Session.State.IsUnlocked())
        {
            _entities.Lock.Press();
        }
    }

    private static bool IsOnline(bool switchState, bool sessionState) => switchState || sessionState;

    private void AddLogoffSchedules(ILaptopScheduler scheduler)
    {
        var logoffSchedules = scheduler.GetSchedules(() =>
        {
            if (!IsOn())
            {
                Logger.LogDebug("Scheduled logoff triggered: Laptop is not on, executing TurnOff.");
                TurnOff();
            }
            else
            {
                Logger.LogDebug("Scheduled logoff triggered: Laptop is currently on, skipping TurnOff.");
            }
        });
        foreach (var schedule in logoffSchedules)
        {
            Automations.Add(schedule);
        }
    }
}
