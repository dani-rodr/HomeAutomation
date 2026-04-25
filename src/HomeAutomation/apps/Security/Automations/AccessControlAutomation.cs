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

    private const int DOOR_CLOSE_WINDOW_DELAY = 5;
    private const int UNLOCK_SUPPRESION_DELAY = 10;
    private volatile bool _doorRecentlyOpened = false;
    private volatile bool _pendingDepartureLock = false;
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
                .OnDeparted(new(StartImmediately: false))
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
                .OnOpened()
                .Subscribe(_ =>
                {
                    Logger.LogDebug("Door opened. Marking door as recently opened.");
                    _doorRecentlyOpened = true;
                }),
            _door
                .OnClosed()
                .Subscribe(_ =>
                {
                    Logger.LogDebug("Door closed.");
                    if (_pendingDepartureLock)
                    {
                        Logger.LogInformation(
                            "Door closed after a confirmed departure. Locking now."
                        );
                        _lock.Lock();
                        _pendingDepartureLock = false;
                    }
                }),
            _door
                .OnClosed(new(Minutes: DOOR_CLOSE_WINDOW_DELAY))
                .Subscribe(_ =>
                {
                    Logger.LogDebug(
                        "Door has been closed for {Delay} minutes. Clearing recent door interaction flags.",
                        DOOR_CLOSE_WINDOW_DELAY
                    );
                    _doorRecentlyOpened = false;
                    _pendingDepartureLock = false;
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

        person.SetHome();
        Logger.LogDebug("{PersonName} is now home", person.Name);

        if (_wasHouseEmpty)
        {
            Logger.LogInformation("House was empty. Unlocking once for {PersonName}", person.Name);
            _lock.Unlock();
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
        Logger.LogInformation("House occupied. Unlocking for {PersonName}", person.Name);
    }

    private void OnDeparture(IPersonController person, string triggerEntityId)
    {
        Logger.LogInformation(
            "{PersonName} away trigger activated: {TriggerEntity}",
            person.Name,
            triggerEntityId
        );

        if (!_doorRecentlyOpened)
        {
            Logger.LogInformation(
                "{PersonName} away trigger ignored — door was not opened recently",
                person.Name
            );
            return;
        }

        person.SetAway();

        if (_door.IsOpen())
        {
            Logger.LogInformation(
                "{PersonName} is away while the door is still open. Locking will happen on close.",
                person.Name
            );
            _pendingDepartureLock = true;
            return;
        }

        Logger.LogInformation(
            "{PersonName} is now away and the door is already closed. Locking now.",
            person.Name
        );
        _lock.Lock();
    }
}
