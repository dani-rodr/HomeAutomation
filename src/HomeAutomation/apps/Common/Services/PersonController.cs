using System.Linq;
using System.Reactive.Subjects;

namespace HomeAutomation.apps.Common.Services;

public interface IPersonController : IAutomation
{
    void SetHome();
    void SetAway();
    string Name { get; }
    IObservable<string> OnArrived(BinaryDuration? duration = null);
    IObservable<string> OnDeparted(BinaryDuration? duration = null);
    IObservable<string> OnUnlocked(BinaryDuration? duration = null);
}

public class PersonController(IPersonEntities entities, ILogger logger)
    : AutomationBase(logger),
        IPersonController
{
    private readonly InputBooleanEntity _presence = entities.Presence;
    private readonly CounterEntity _counter = entities.Counter;
    private readonly ButtonEntity _toggle = entities.ToggleLocation;
    private readonly Subject<string> _arrivedHomeSubject = new();
    private readonly Subject<string> _leftHomeSubject = new();

    public IObservable<string> OnArrived(BinaryDuration? duration = null) =>
        entities
            .HomeTriggers.OnTurnedOn(duration)
            .Where(_ => _presence.IsOff())
            .Select(trigger => trigger.Entity.EntityId)
            .Merge(_arrivedHomeSubject);

    public IObservable<string> OnDeparted(BinaryDuration? duration = null) =>
        entities
            .AwayTriggers.OnTurnedOff(duration)
            .Where(_ => _presence.IsOn())
            .Select(trigger => trigger.Entity.EntityId)
            .Merge(_leftHomeSubject);

    public IObservable<string> OnUnlocked(BinaryDuration? duration = null) =>
        entities
            .DirectUnlockTriggers.OnTurnedOn(duration)
            .Select(trigger => trigger.Entity.EntityId);

    public string Name => entities.Name;

    public void SetHome()
    {
        if (_presence.IsOff())
        {
            Logger.LogInformation(
                "{PersonName} arrived home. Updating location and incrementing counter.",
                Name
            );
            _presence.TurnOn();
            _counter.Increment();
        }
    }

    public void SetAway()
    {
        if (_presence.IsOn())
        {
            Logger.LogInformation(
                "{PersonName} left home. Updating location and decrementing counter.",
                Name
            );
            _presence.TurnOff();
            _counter.Decrement();
        }
    }

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [_toggle.OnPressed().Subscribe(ToggleLocation)];

    private void ToggleLocation(StateChange e)
    {
        Logger.LogInformation("Toggle button pressed. Current state: {State}", _presence.State);
        if (_presence.IsOn())
        {
            SetAway();
            _leftHomeSubject.OnNext(_presence.EntityId);
        }
        else
        {
            SetHome();
            _arrivedHomeSubject.OnNext(_presence.EntityId);
        }
    }
}
