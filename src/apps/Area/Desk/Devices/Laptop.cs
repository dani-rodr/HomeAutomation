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

    public override bool IsOn()
    {
        var switchState = _entities.VirtualSwitch;
        var sessionState = _entities.Session.State;

        if (switchState.IsOff())
        {
            return false;
        }

        if (sessionState.IsLocked())
        {
            return false;
        }
        return true;
    }

    public override IObservable<bool> StateChanges()
    {
        var switchStateChanges = _entities
            .VirtualSwitch.StateChanges()
            .Select(e => e.IsOn())
            .StartWith(_entities.VirtualSwitch.State.IsOn());

        var sessionLocked = _entities
            .Session.StateChanges()
            .Where(e => e.Old?.State.IsUnlocked() == true && e.New?.State.IsLocked() == true)
            .Select(_ => false);

        // Emits true when switch turns on, false when switch turns off or session locks
        return switchStateChanges.Merge(sessionLocked).DistinctUntilChanged();
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
