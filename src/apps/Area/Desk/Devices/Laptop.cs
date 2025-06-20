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
        Automations.Add(_batteryHandler.StartMonitoring());
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

        var sessionLocked = _entities.Session.StateChanges().IsLocked().Select(e => false);

        // Combine both streams to determine whether the laptop is considered online
        return Observable.CombineLatest(switchOn, sessionLocked, IsOnline).DistinctUntilChanged();
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

    private bool GetSessionState(StateChange e)
    {
        if (e.New is null)
        {
            return _entities.VirtualSwitch.State.IsOn();
        }
        if (e.New.State.IsUnlocked())
        {
            return true;
        }
        if (e.New.State.IsLocked())
        {
            return false;
        }
        return _entities.VirtualSwitch.State.IsOn();
    }

    private static bool IsOnline(bool switchState, bool sessionState) => switchState && sessionState;

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
