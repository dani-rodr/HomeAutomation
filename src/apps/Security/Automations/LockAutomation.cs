using System.Reactive;

namespace HomeAutomation.apps.Common.Security.Automations;

public class LockAutomation(
    ILockingEntities entities,
    INotificationServices services,
    IEventHandler eventHandler,
    ILogger logger
) : AutomationBase(logger, entities.MasterSwitch)
{
    private const string LOCK_TAG = "lock";
    private const string LOCK_ACTION = "LOCK_ACTION";
    private bool _isImmediateRelock = false;
    private const int AUTO_UNLOCK_IN_MINUTES = 5;

    protected override IEnumerable<IDisposable> GetPersistentAutomations() => [];

    protected override IEnumerable<IDisposable> GetToggleableAutomations()
    {
        var lockChanges = entities.Lock.StateChanges();
        var doorChanges = entities.Door.StateChanges();

        yield return lockChanges.IsLocked().Subscribe(HandleDoorLocked);
        yield return lockChanges.IsUnlocked().Subscribe(HandleDoorUnlocked);

        yield return doorChanges.IsClosed().Subscribe(HandleDoorClosed);
        yield return doorChanges.IsOpen().Subscribe(SendDoorOpenedNotification);

        yield return AutoLockOnClosedTimer();

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
        if (entities.Lock.IsLocked())
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
        (entities.MotionSensor.IsOn() || entities.HouseStatus.IsOff()) && entities.Lock.IsUnlocked();

    private IDisposable AutoLockOnClosedTimer()
    {
        return entities
            .Lock.StateChanges()
            .IsUnlockedForMinutes(AUTO_UNLOCK_IN_MINUTES)
            .Where(_ => ShouldAutoLockAfterTime)
            .SelectMany(_ => entities.Door.StateChangesWithCurrent().IsClosed().Take(1))
            .Subscribe(Lock);
    }

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
        if (e.IsPhysicallyOperated())
        {
            _isImmediateRelock = false;
            return;
        }
        if (e.IsManuallyOperated())
        {
            _isImmediateRelock = true;
        }
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

    private void SendUnlockedNotification(StateChange e) =>
        services.NotifyPocoF4(
            message: "Door is unlocked",
            data: GetLockNotificationData("mdi:lock-open-variant", "Lock"),
            title: "Home Assistant"
        );

    private void SendDoorOpenedNotification(StateChange e) =>
        services.NotifyPocoF4(message: "Door is opened", data: GetDoorNotificationData(), title: "Home Assistant");

    private static object GetBaseNotificationData(string icon, object? actions = null) =>
        new
        {
            tag = LOCK_TAG,
            clickAction = "entityId:lock.front_door_2",
            visibility = "public",
            notification_icon = icon,
            persistent = true,
            sticky = true,
            actions = actions,
        };

    private static object GetLockNotificationData(string icon, string actionTitle) =>
        GetBaseNotificationData(icon, new[] { new { action = LOCK_ACTION, title = actionTitle } });

    private static object GetDoorNotificationData() => GetBaseNotificationData("mdi:door-open");
}
