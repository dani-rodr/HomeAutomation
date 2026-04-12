using HomeAutomation.apps.Security.Automations.Entities;

namespace HomeAutomation.apps.Security.Automations;

public class AccessControlAutomation(
    IEnumerable<IPersonController> personControllers,
    IAccessControlAutomationEntities entities,
    ILogger<AccessControlAutomation> logger
) : AutomationBase(logger)
{
    private readonly IEnumerable<IPersonController> _personControllers = personControllers;
    private readonly BinarySensorEntity _door = entities.Door;
    private readonly LockEntity _lock = entities.Lock;
    private readonly Lock _pendingDeparturesSync = new();
    private readonly HashSet<IPersonController> _pendingDepartures = [];

    private const int LOCK_ON_AWAY_DELAY = 0;
    private const int UNLOCK_SUPPRESION_DELAY = 10;
    private volatile bool _autoLockOnDoorClose = false;
    private volatile bool _wasHouseEmpty = false;
    private volatile bool _suppressUnlocks = false;

    protected override IEnumerable<IDisposable> GetAutomations() =>
        [
            .. GetPersonAccessAutomations(),
            .. GetDoorAutoLockAutomations(),
            .. GetLockSuppressionDelayAutomation(),
            entities
                .House.OnCleared()
                .Subscribe(_ =>
                {
                    Logger.LogInformation("House became empty.");
                    _wasHouseEmpty = true;
                }),
        ];

    private IEnumerable<IDisposable> GetPersonAccessAutomations()
    {
        Logger.LogDebug("AccessControlAutomation initialized with person controllers");

        foreach (var person in _personControllers)
        {
            yield return person
                .OnArrived(new(StartImmediately: false))
                .Subscribe(triggerId => OnArrival(person, triggerId));
            yield return person
                .OnDeparted(new(StartImmediately: false, Seconds: LOCK_ON_AWAY_DELAY))
                .Subscribe(triggerId => OnDeparture(person, triggerId));
            yield return person
                .OnUnlocked(new(StartImmediately: false))
                .Subscribe(triggerId =>
                {
                    Logger.LogInformation(
                        "{PersonName} direct unlock trigger activated: {TriggerEntity}",
                        person.Name,
                        triggerId
                    );
                    _lock.Unlock();
                    person.SetHome();
                });
        }
    }

    private IEnumerable<IDisposable> GetDoorAutoLockAutomations() =>
        [
            _door
                .OnClosed()
                .Subscribe(_ =>
                {
                    Logger.LogDebug("Door closed.");
                    var completedPendingDepartures = CompletePendingDepartures();
                    if (_autoLockOnDoorClose || completedPendingDepartures)
                    {
                        _lock.Lock();
                        _autoLockOnDoorClose = false;
                    }
                }),
        ];

    private IEnumerable<IDisposable> GetLockSuppressionDelayAutomation() =>
        [
            entities
                .House.OnOccupied(new(StartImmediately: false))
                .Subscribe(_ =>
                {
                    Logger.LogInformation(
                        "House occupied. Suppressing unlocks for {Delay} minutes.",
                        UNLOCK_SUPPRESION_DELAY
                    );
                    _suppressUnlocks = true;
                }),
            entities
                .House.OnOccupied(new(Minutes: UNLOCK_SUPPRESION_DELAY))
                .Subscribe(_ =>
                {
                    Logger.LogDebug("Unlock suppression window expired. Re-enabling unlocks.");
                    _suppressUnlocks = false;
                }),
        ];

    private void OnArrival(IPersonController person, string triggerEntityId)
    {
        Logger.LogInformation(
            "{PersonName} home trigger activated: {TriggerEntity}",
            person.Name,
            triggerEntityId
        );

        CancelPendingDeparture(person);
        person.SetHome();
        Logger.LogDebug("{PersonName} is now home", person.Name);

        if (_wasHouseEmpty)
        {
            Logger.LogInformation("House was empty. Unlocking once for {PersonName}", person.Name);
            _lock.Unlock();
            _autoLockOnDoorClose = true;
            _wasHouseEmpty = false;

            return;
        }

        if (_suppressUnlocks is true)
        {
            Logger.LogInformation(
                "Suppression active. Ignoring unlock for {PersonName}",
                person.Name
            );
            return;
        }

        _lock.Unlock();
        _autoLockOnDoorClose = true;
        Logger.LogInformation(
            "House occupied. Unlocking for {PersonName}, Setting Auto Lock on Door Closed : {value}",
            person.Name,
            _autoLockOnDoorClose
        );
    }

    private void OnDeparture(IPersonController person, string triggerEntityId)
    {
        Logger.LogInformation(
            "{PersonName} away trigger activated after {LockDelay}s delay: {TriggerEntity}",
            person.Name,
            LOCK_ON_AWAY_DELAY,
            triggerEntityId
        );

        if (_door.IsClosed())
        {
            person.SetAway();
            Logger.LogInformation(
                "{PersonName} is now away, door already closed, locking door",
                person.Name
            );
            _lock.Lock();
            return;
        }

        AddPendingDeparture(person);
        Logger.LogInformation(
            "{PersonName} departure detected, waiting for door to close before marking away and locking",
            person.Name
        );
    }

    private void AddPendingDeparture(IPersonController person)
    {
        lock (_pendingDeparturesSync)
        {
            _pendingDepartures.Add(person);
        }
    }

    private void CancelPendingDeparture(IPersonController person)
    {
        lock (_pendingDeparturesSync)
        {
            _pendingDepartures.Remove(person);
        }
    }

    private bool CompletePendingDepartures()
    {
        IPersonController[] pendingDepartures;
        lock (_pendingDeparturesSync)
        {
            if (_pendingDepartures.Count == 0)
            {
                return false;
            }

            pendingDepartures = [.. _pendingDepartures];
            _pendingDepartures.Clear();
        }

        foreach (var pendingDeparture in pendingDepartures)
        {
            pendingDeparture.SetAway();
        }

        return true;
    }
}
