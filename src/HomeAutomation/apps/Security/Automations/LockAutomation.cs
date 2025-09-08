namespace HomeAutomation.apps.Common.Security.Automations;

public class LockAutomation(
    ILockingEntities entities,
    INotificationServices services,
    IEventHandler eventHandler,
    ILogger<LockAutomation> logger
) : ToggleableAutomation(entities.MasterSwitch, logger)
{
    private const string LOCK_TAG = "lock";
    private const string LOCK_ACTION = "LOCK_ACTION";
    private bool _isImmediateRelock = false;
    private const int AUTO_LOCK_IN_MINUTES = 5;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        var @lock = entities.Lock;
        var door = entities.Door;

        yield return @lock.OnLocked().Subscribe(HandleDoorLocked);
        yield return @lock.OnUnlocked().Subscribe(HandleDoorUnlocked);
        yield return @lock
            .OnUnlocked(new(Minutes: AUTO_LOCK_IN_MINUTES))
            .Where(_ => entities.Door.IsClosed() && ShouldAutoLockAfterTime)
            .Subscribe(Lock);
        yield return door.OnClosed(new(StartImmediately: false)).Subscribe(HandleDoorClosed);
        yield return door.OnOpened().Subscribe(SendDoorOpenedNotification);
        yield return door.OnOpened(new(Minutes: AUTO_LOCK_IN_MINUTES))
            .Subscribe(SendDoorOpenedNotification);

        yield return eventHandler.OnMobileEvent(LOCK_ACTION).Subscribe(_ => entities.Lock.Lock());
        yield return eventHandler
            .OnNfcScan(NFC_ID.DOOR_LOCK)
            .Where(e => !HaIdentity.IsPhysicallyOperated(e))
            .Subscribe(ToggleLock);
    }

    private void ToggleLock(string userId)
    {
        if (entities.Lock.IsUnlocked())
        {
            entities.Lock.Lock();
        }
        else if (entities.Lock.IsLocked())
        {
            entities.Lock.Unlock();
        }
    }

    private void Lock(StateChange e)
    {
        if (entities.Lock.IsUnlocked())
        {
            entities.Lock.Lock();
        }
    }

    private bool ShouldAutoLockAfterTime =>
        (entities.MotionSensor.IsOn() || entities.HouseStatus.IsOff())
        && entities.Lock.IsUnlocked();

    private void HandleDoorLocked(StateChange e)
    {
        entities.Flytrap.TurnOff();
        _isImmediateRelock = false;
        ClearLockNotification(e);
    }

    private void HandleDoorUnlocked(StateChange e)
    {
        entities.Flytrap.TurnOn();
        SendUnlockedNotification(e);
        _isImmediateRelock = !e.IsPhysicallyOperated();
    }

    private void HandleDoorClosed(StateChange e)
    {
        if (_isImmediateRelock)
        {
            Lock(e);
        }
        else
        {
            SendUnlockedNotification(e);
        }
    }

    private void ClearLockNotification(StateChange e) =>
        services.NotifyPocoF4(message: "clear_notification", data: new { tag = LOCK_TAG });

    private void SendUnlockedNotification(StateChange e)
    {
        var message = $"Door was unlocked by {e.Username()}";

        if (e.IsPhysicallyOperated())
        {
            message = "Door was physically unlocked";
        }

        services.NotifyPocoF4(
            message: message,
            data: GetBaseNotificationData(
                "mdi:lock-open-variant",
                new[] { new { action = LOCK_ACTION, title = "Lock" } }
            ),
            title: "Home Assistant"
        );
    }

    private void SendDoorOpenedNotification(StateChange e) =>
        services.NotifyPocoF4(
            message: "Door is opened",
            data: GetBaseNotificationData("mdi:door-open"),
            title: "Home Assistant"
        );

    private static object GetBaseNotificationData(string icon, object? actions = null) =>
        new
        {
            tag = LOCK_TAG,
            clickAction = "entityId:lock.front_door_2",
            visibility = "public",
            notification_icon = icon,
            persistent = true,
            sticky = true,
            actions,
        };
}
