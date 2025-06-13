using System.Linq;
using System.Reactive.Disposables;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop : ComputerBase, IDisposable
{
    protected override string ShowEvent { get; } = "show_laptop";
    protected override string HideEvent { get; } = "hide_laptop";
    private readonly ILaptopEntities _entities;
    private readonly CompositeDisposable _disposables = [];

    public Laptop(ILaptopEntities entities, IEventHandler eventHandler, ILogger logger)
        : base(eventHandler, logger)
    {
        _entities = entities;
        _disposables =
        [
            _entities
                .Switch.StateChanges()
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
                }),
        ];
    }

    public override bool IsOn() => IsOnline(_entities.Switch.IsOn(), _entities.Session.IsUnlocked());

    public override IObservable<bool> StateChanges()
    {
        // Observables for both switch and session state changes
        var switchOn = _entities.Switch.StateChanges().Select(e => e.IsOn()).StartWith(_entities.Switch.State.IsOn());

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
        _entities.Switch.TurnOn();
        _entities.PowerPlug.TurnOn();
        foreach (var button in _entities.WakeOnLanButtons)
        {
            button.Press();
        }
    }

    public override void TurnOff()
    {
        _entities.Switch.TurnOff();

        if (_entities.Session.State.IsUnlocked())
        {
            _entities.Lock.Press();
        }
    }

    private static bool IsOnline(bool switchState, bool sessionState) => switchState || sessionState;

    public void Dispose()
    {
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}
