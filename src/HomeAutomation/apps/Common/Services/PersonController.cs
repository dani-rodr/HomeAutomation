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

public class PersonController(IPersonEntities entities, IServices services, ILogger logger)
    : AutomationBase(logger),
        IPersonController
{
    private readonly PersonEntity _person = entities.Person;
    private readonly CounterEntity _counter = entities.Counter;
    private readonly ButtonEntity _toggle = entities.ToggleLocation;
    private readonly Subject<string> _arrivedHomeSubject = new();
    private readonly Subject<string> _leftHomeSubject = new();

    public IObservable<string> OnArrived(BinaryDuration? duration = null) =>
        entities
            .HomeTriggers.OnTurnedOn(duration)
            .Where(_ => _person.IsAway())
            .Select(trigger => trigger.Entity.EntityId)
            .Merge(_arrivedHomeSubject);

    public IObservable<string> OnDeparted(BinaryDuration? duration = null) =>
        entities
            .AwayTriggers.OnTurnedOff(duration)
            .Where(_ => _person.IsHome())
            .Select(trigger => trigger.Entity.EntityId)
            .Merge(_leftHomeSubject);

    public IObservable<string> OnUnlocked(BinaryDuration? duration = null) =>
        entities
            .DirectUnlockTriggers.OnTurnedOn(duration)
            .Select(trigger => trigger.Entity.EntityId);

    public string Name => _person.Attributes?.FriendlyName ?? "Unknown";

    public void SetHome()
    {
        if (_person.IsAway())
        {
            Logger.LogInformation(
                "{PersonName} arrived home. Updating location and incrementing counter.",
                _person.Attributes?.FriendlyName ?? "Unknown person"
            );
            SetLocation(HaEntityStates.HOME);
            _counter.Increment();
        }
    }

    public void SetAway()
    {
        if (_person.IsHome())
        {
            Logger.LogInformation(
                "{PersonName} left home. Updating location and decrementing counter.",
                _person.Attributes?.FriendlyName ?? "Unknown person"
            );
            SetLocation(HaEntityStates.AWAY);
            _counter.Decrement();
        }
    }

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [_toggle.OnPressed().Subscribe(ToggleLocation)];

    private void ToggleLocation(StateChange e)
    {
        Logger.LogInformation("Toggle button pressed. Current state: {State}", _person.State);
        if (_person.IsHome())
        {
            SetAway();
            _leftHomeSubject.OnNext(_person.EntityId);
        }
        else
        {
            SetHome();
            _arrivedHomeSubject.OnNext(_person.EntityId);
        }
    }

    private void SetLocation(string location) =>
        services.DeviceTracker.See(devId: _person.Attributes?.Id, locationName: location);
}
