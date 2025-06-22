using System.Linq;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop(
    ILaptopEntities entities,
    ILaptopScheduler scheduler,
    IBatteryHandler batteryHandler,
    IEventHandler eventHandler,
    ILogger logger
) : ComputerBase(eventHandler, logger)
{
    protected override string ShowEvent { get; } = "show_laptop";
    protected override string HideEvent { get; } = "hide_laptop";

    protected override CompositeDisposable Automations =>
        [
            GetSwitchToggleAutomations(),
            batteryHandler.StartMonitoring(),
            .. GetLogoffAutomations(scheduler),
        ];

    public override bool IsOn()
    {
        var switchState = entities.VirtualSwitch;
        var sessionState = entities.Session.State;

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
        var switchStateChanges = entities
            .VirtualSwitch.StateChanges()
            .Select(e => e.IsOn())
            .StartWith(entities.VirtualSwitch.State.IsOn());

        var sessionLocked = entities
            .Session.StateChanges()
            .Where(e => e.Old?.State.IsUnlocked() == true && e.New?.State.IsLocked() == true)
            .Select(_ => false);

        // Emits true when switch turns on, false when switch turns off or session locks
        return switchStateChanges.Merge(sessionLocked).DistinctUntilChanged();
    }

    public override void TurnOn()
    {
        entities.VirtualSwitch.TurnOn();
        batteryHandler.HandleLaptopTurnedOn();
        foreach (var button in entities.WakeOnLanButtons)
        {
            button.Press();
        }
    }

    public override void TurnOff()
    {
        _ = batteryHandler.HandleLaptopTurnedOffAsync();
        entities.VirtualSwitch.TurnOff();

        if (entities.Session.State.IsUnlocked())
        {
            entities.Lock.Press();
        }
    }

    private IDisposable GetSwitchToggleAutomations() =>
        entities
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

    private IEnumerable<IDisposable> GetLogoffAutomations(ILaptopScheduler scheduler) =>
        scheduler.GetSchedules(() =>
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
}
