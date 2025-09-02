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

    private const int LOCK_ON_AWAY_DELAY = 60;
    private const int DOOR_CLOSE_WINDOW_DELAY = 5;
    private const int UNLOCK_SUPPRESION_DELAY = 10;
    private volatile bool _doorRecentlyClosed = false;
    private volatile bool _wasHouseEmpty = false;
    private IDisposable? _suppressUnlocks;

    protected override IEnumerable<IDisposable> GetAutomations()
    {
        Logger.LogInformation("AccessControlAutomation initialized with person controllers");

        foreach (var person in _personControllers)
        {
            yield return person.ArrivedHome.Subscribe(triggerId =>
                OnHomeTriggerActivated(person, triggerId)
            );
            yield return person.LeftHome.Subscribe(triggerId =>
                OnAwayTriggerActivated(person, triggerId)
            );
            yield return person.DirectUnlock.Subscribe(triggerId =>
            {
                Logger.LogInformation(
                    "{PersonName} direct unlock trigger activated: {TriggerEntity}",
                    person.Name,
                    triggerId
                );
                _lock.Unlock();
            });
        }
        yield return _door.OnClosed().Subscribe(_ => _doorRecentlyClosed = true);
        yield return _door
            .StateChanges()
            .IsClosed()
            .ForMinutes(DOOR_CLOSE_WINDOW_DELAY)
            .Subscribe(_ => _doorRecentlyClosed = false);
        yield return entities
            .House.StateChanges()
            .IsOff()
            .Subscribe(_ =>
            {
                Logger.LogInformation("House became empty.");
                _wasHouseEmpty = true;
                _suppressUnlocks?.Dispose();
                _suppressUnlocks = null;
            });
    }

    private void OnHomeTriggerActivated(IPersonController person, string triggerEntityId)
    {
        Logger.LogInformation(
            "{PersonName} home trigger activated: {TriggerEntity}",
            person.Name,
            triggerEntityId
        );

        person.SetHome();
        Logger.LogInformation("{PersonName} is now home", person.Name);

        if (_wasHouseEmpty && _suppressUnlocks == null)
        {
            Logger.LogInformation("House was empty. Unlocking once for {PersonName}", person.Name);
            _lock.Unlock();

            _wasHouseEmpty = false;

            _suppressUnlocks = Observable
                .Timer(TimeSpan.FromMinutes(UNLOCK_SUPPRESION_DELAY), SchedulerProvider.Current)
                .Subscribe(_ =>
                {
                    Logger.LogInformation("Unlock suppression window ended.");
                    _suppressUnlocks = null;
                });

            return;
        }

        if (_suppressUnlocks != null)
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

    private void OnAwayTriggerActivated(IPersonController person, string triggerEntityId)
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
