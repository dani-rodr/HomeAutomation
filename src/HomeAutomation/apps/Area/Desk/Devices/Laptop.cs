using System.Linq;
using System.Reactive.Disposables;
using HomeAutomation.apps.Area.Desk.Devices.Entities;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop(
    ILaptopEntities entities,
    ILaptopShutdownScheduler scheduler,
    ILaptopChargingHandler batteryHandler,
    IEventHandler eventHandler,
    ILogger<Laptop> logger
) : ComputerBase(eventHandler, logger), ILaptop
{
    private readonly SerialDisposable _pendingMotionOffShutdown = new();

    protected override string ShowEvent { get; } = "show_laptop";
    protected override string HideEvent { get; } = "hide_laptop";

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [
            GetSwitchToggleAutomations(),
            .. GetSessionLockSwitchAutomation(),
            GetSessionUnlockSwitchAutomation(),
            batteryHandler.StartMonitoring(),
            _pendingMotionOffShutdown,
            .. GetLogoffAutomations(scheduler),
        ];

    public override bool IsOn()
    {
        var switchState = entities.VirtualSwitch;
        var sessionState = entities.Session;

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
        var switchStateChanges = entities.VirtualSwitch.StateChanges().Select(e => e.IsOn());

        var sessionLocked = entities
            .Session.OnLocked(new(AllowFromUnavailable: false))
            .Select(_ => false);

        // Emits true when switch turns on, false when switch turns off or session locks
        return switchStateChanges.Merge(sessionLocked).DistinctUntilChanged();
    }

    public override void TurnOn()
    {
        entities.VirtualSwitch.TurnOn();
        batteryHandler.HandleLaptopTurnedOn();
        entities.WakeOnLanButton.Press();
    }

    public override void TurnOff()
    {
        batteryHandler.HandleLaptopTurnedOff();
        entities.VirtualSwitch.TurnOff();

        if (entities.Session.IsUnlocked())
        {
            entities.Lock.Press();
        }
    }

    private IDisposable GetSwitchToggleAutomations() =>
        entities
            .VirtualSwitch.OnChanges()
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

    private IEnumerable<IDisposable> GetLogoffAutomations(ILaptopShutdownScheduler scheduler) =>
        scheduler.GetSchedules(() =>
        {
            _pendingMotionOffShutdown.Disposable = null;

            if (!IsOn())
            {
                Logger.LogDebug("Laptop is not on, skipping TurnOff.");
                return;
            }

            if (entities.MotionSensor.IsOff())
            {
                Logger.LogDebug("Motion sensor is already off, proceeding to TurnOff.");
                TurnOff();
                return;
            }

            Logger.LogDebug("Motion sensor is on, waiting for it to turn off.");

            _pendingMotionOffShutdown.Disposable = entities
                .MotionSensor.OnTurnedOff()
                .Take(1)
                .Subscribe(_ =>
                {
                    Logger.LogDebug(
                        "Motion sensor turned off after schedule, proceeding to TurnOff."
                    );
                    TurnOff();
                    _pendingMotionOffShutdown.Disposable = null;
                });
        });

    private IEnumerable<IDisposable> GetSessionLockSwitchAutomation() =>
        [
            entities
                .Session.OnLocked()
                .Where(s => s.Old.IsUnlocked())
                .Subscribe(_ =>
                {
                    Logger.LogInformation(
                        "Session locked. Automatically turning off laptop virtual switch."
                    );
                    entities.VirtualSwitch.TurnOff();
                }),
        ];

    private IDisposable GetSessionUnlockSwitchAutomation() =>
        entities
            .Session.OnUnlocked()
            .Subscribe(_ =>
            {
                Logger.LogInformation(
                    "Session unlocked. Automatically turning on laptop virtual switch."
                );
                entities.VirtualSwitch.TurnOn();
            });
}
