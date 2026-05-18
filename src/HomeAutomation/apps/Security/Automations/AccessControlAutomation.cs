using HomeAutomation.apps.Security.Automations.Entities;

namespace HomeAutomation.apps.Security.Automations;

public class AccessControlAutomation(
    IEnumerable<IPersonController> personControllers,
    IAccessControlAutomationEntities entities,
    ILogger<AccessControlAutomation> logger
) : AutomationBase(logger)
{
    private enum DoorCloseAction
    {
        None,
        LockAfterArrival,
        LockAfterDeparture,
    }

    private readonly IEnumerable<IPersonController> _personControllers = personControllers;
    private readonly BinarySensorEntity _door = entities.Door;
    private readonly LockEntity _lock = entities.Lock;

    private const int DOOR_CLOSED_STABILITY_DELAY_SECONDS = 3;
    private const int DOOR_CLOSE_WINDOW_DELAY = 5;
    private const int UNLOCK_SUPPRESION_DELAY = 10;
    private volatile bool _doorRecentlyOpened = false;
    private volatile bool _doorClosedAfterRecentOpen = false;
    private volatile bool _waitingForArrivalDoorOpen = false;
    private volatile bool _wasHouseEmpty = false;
    private volatile bool _suppressUnlocks = false;
    private volatile DoorCloseAction _doorCloseAction = DoorCloseAction.None;

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
            _door.OnOpened().Subscribe(_ => HandleDoorOpened()),
            _door
                .OnClosed(new(Seconds: DOOR_CLOSED_STABILITY_DELAY_SECONDS))
                .Subscribe(_ => HandleDoorClosed()),
            _door
                .OnClosed(new(Minutes: DOOR_CLOSE_WINDOW_DELAY))
                .Subscribe(_ => ClearDoorInteractionState()),
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
            UnlockForArrival(person, wasHouseEmpty: true);
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

        UnlockForArrival(person, wasHouseEmpty: false);
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

        if (!_doorClosedAfterRecentOpen)
        {
            Logger.LogInformation(
                "{PersonName} is away before the door close was observed. Locking will happen on close.",
                person.Name
            );
            _doorCloseAction = DoorCloseAction.LockAfterDeparture;
            return;
        }

        Logger.LogInformation(
            "{PersonName} is now away and the door is already closed. Locking now.",
            person.Name
        );
        _lock.Lock();
        _doorCloseAction = DoorCloseAction.None;
    }

    private void UnlockForArrival(IPersonController person, bool wasHouseEmpty)
    {
        var context = wasHouseEmpty ? "House was empty" : "House occupied";
        Logger.LogInformation("{Context}. Unlocking for {PersonName}", context, person.Name);
        _lock.Unlock();
        _waitingForArrivalDoorOpen = true;
        _doorCloseAction = _door.IsOpen() ? DoorCloseAction.LockAfterArrival : DoorCloseAction.None;
    }

    private void HandleDoorOpened()
    {
        Logger.LogDebug("Door opened. Marking door as recently opened.");
        _doorRecentlyOpened = true;
        _doorClosedAfterRecentOpen = false;

        if (_waitingForArrivalDoorOpen)
        {
            _doorCloseAction = DoorCloseAction.LockAfterArrival;
        }
    }

    private void HandleDoorClosed()
    {
        Logger.LogDebug("Door closed.");
        _doorClosedAfterRecentOpen = true;

        if (_doorCloseAction is DoorCloseAction.LockAfterDeparture)
        {
            Logger.LogInformation("Door closed after a confirmed departure. Locking now.");
            _lock.Lock();
            _doorCloseAction = DoorCloseAction.None;
            _doorRecentlyOpened = false;
            return;
        }

        if (_doorCloseAction is DoorCloseAction.LockAfterArrival)
        {
            Logger.LogInformation("Door closed after an arrival unlock. Locking now.");
            _lock.Lock();
            _waitingForArrivalDoorOpen = false;
            _doorCloseAction = DoorCloseAction.None;
            _doorRecentlyOpened = false;
        }
    }

    private void ClearDoorInteractionState()
    {
        Logger.LogDebug(
            "Door has been closed for {Delay} minutes. Clearing recent door interaction flags.",
            DOOR_CLOSE_WINDOW_DELAY
        );
        _doorRecentlyOpened = false;
        _doorClosedAfterRecentOpen = false;
        _waitingForArrivalDoorOpen = false;
        _doorCloseAction = DoorCloseAction.None;
    }
}
