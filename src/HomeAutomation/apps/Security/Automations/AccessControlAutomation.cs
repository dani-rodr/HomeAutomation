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

    private const int LOCK_ON_AWAY_DELAY = 0;
    private const int DOOR_CLOSE_WINDOW_DELAY = 5;
    private const int UNLOCK_SUPPRESION_DELAY = 10;
    private volatile bool _autoLockOnDoorClose = false;
    private volatile bool _doorRecentlyClosed = false;
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
        Logger.LogInformation("AccessControlAutomation initialized with person controllers");

        foreach (var person in _personControllers)
        {
            yield return person.OnArrived().Subscribe(triggerId => OnArrival(person, triggerId));
            yield return person
                .OnDeparted(new(Seconds: LOCK_ON_AWAY_DELAY))
                .Subscribe(triggerId => OnDeparture(person, triggerId));
            yield return person
                .OnUnlocked()
                .Subscribe(triggerId =>
                {
                    Logger.LogInformation(
                        "{PersonName} direct unlock trigger activated: {TriggerEntity}",
                        person.Name,
                        triggerId
                    );
                    _lock.Unlock();
                });
        }
    }

    private IEnumerable<IDisposable> GetDoorAutoLockAutomations() =>
        [
            _door
                .OnClosed()
                .Subscribe(_ =>
                {
                    Logger.LogInformation("Door closed. Marking door as recently closed.");
                    _doorRecentlyClosed = true;
                    if (_autoLockOnDoorClose)
                    {
                        _lock.Lock();
                        _autoLockOnDoorClose = false;
                    }
                }),
            _door
                .OnClosed(new(Minutes: DOOR_CLOSE_WINDOW_DELAY))
                .Subscribe(_ =>
                {
                    Logger.LogInformation(
                        "Door has been closed for {Delay} minutes. Clearing 'recently closed' flag.",
                        DOOR_CLOSE_WINDOW_DELAY
                    );
                    _doorRecentlyClosed = false;
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
                    Logger.LogInformation(
                        "Unlock suppression window expired. Re-enabling unlocks."
                    );
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

        person.SetHome();
        Logger.LogInformation("{PersonName} is now home", person.Name);

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

        if (!_doorRecentlyClosed)
        {
            Logger.LogInformation(
                "{PersonName} away trigger ignored — door was not recently closed",
                person.Name
            );
            return;
        }
        _lock.Lock();
        person.SetAway();
        Logger.LogInformation("{PersonName} is now away, locking door", person.Name);
    }
}
