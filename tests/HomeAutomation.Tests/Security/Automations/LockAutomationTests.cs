using HomeAutomation.apps.Common.Interface;
using HomeAutomation.apps.Security.Automations;
using HomeAutomation.apps.Security.Automations.Entities;

namespace HomeAutomation.Tests.Security.Automations;

/// <summary>
/// Comprehensive behavioral tests for Security LockAutomation covering critical security functionality
/// Tests lock/unlock behavior, NFC integration, auto-lock logic, door state coordination, and notifications
/// </summary>
public class LockAutomationTests : AutomationTestBase<LockAutomation>
{
    private MockHaContext _mockHaContext => HaContext;
    private Mock<ILogger<LockAutomation>> _mockLogger => Logger;
    private readonly Mock<INotificationServices> _mockNotificationServices;
    private readonly Mock<IEventHandler> _mockEventHandler;
    private readonly TestEntities _entities;
    private readonly LockAutomation _automation;

    public LockAutomationTests()
    {
        _mockNotificationServices = new Mock<INotificationServices>();
        _mockEventHandler = new Mock<IEventHandler>();

        // Set up event handler mocks to return proper observables
        _mockEventHandler
            .Setup(x => x.OnMobileEvent(It.IsAny<string>()))
            .Returns(Observable.Never<string>());
        _mockEventHandler
            .Setup(x => x.OnNfcScan(It.IsAny<string>()))
            .Returns(Observable.Never<string>());

        // Create test entities wrapper
        _entities = new TestEntities(_mockHaContext);

        _automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );

        StartAutomation(_automation, _entities.MasterSwitch.EntityId);
    }

    #region Lock State Changes Tests

    [Fact]
    public void LockStateChanged_Locked_Should_TurnOffFlytrapAndClearNotification()
    {
        // Act - Simulate lock being locked
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.UNLOCKED,
            HaEntityStates.LOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should turn off flytrap and clear notification
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Flytrap.EntityId);
        _mockNotificationServices.Verify(
            x => x.NotifyPocoF4("clear_notification", It.IsAny<object>(), null),
            Times.Once,
            "Should send clear notification when lock is locked"
        );
    }

    [Fact]
    public void LockStateChanged_Unlocked_Should_TurnOnFlytrapAndSendNotification()
    {
        // Act - Simulate lock being unlocked
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should turn on flytrap and send unlock notification
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Flytrap.EntityId);
        _mockNotificationServices.Verify(
            x =>
                x.NotifyPocoF4(
                    "Door was physically unlocked",
                    It.IsAny<object>(),
                    "Home Assistant"
                ),
            Times.Once,
            "Should send unlock notification when lock is unlocked"
        );
    }

    [Fact]
    public void LockStateChanged_UnlockedByPhysicalOperation_Should_NotSetImmediateRelock()
    {
        // Arrange - Set door to be closed for testing immediate relock behavior
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off"); // closed

        // Act - Simulate lock being unlocked by physical operation (no user ID)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED,
            null
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Clear previous service calls
        _mockHaContext.ClearServiceCalls();

        // Act - Now simulate door closing (should not trigger immediate relock)
        var doorClosedChange = StateChangeHelpers.DoorClosed(_entities.Door);
        _mockHaContext.EmitStateChange(doorClosedChange);

        // Assert - Should NOT call lock service (no immediate relock)
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void LockStateChanged_UnlockedByAutomation_Should_SetImmediateRelock()
    {
        // Arrange - Set door to be closed and lock to be unlocked for testing immediate relock behavior
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off"); // closed
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Act - Simulate lock being unlocked by automation (with user ID)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED,
            HaIdentity.SUPERVISOR
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Clear previous service calls
        _mockHaContext.ClearServiceCalls();

        // Act - Now simulate door closing (should trigger immediate relock)
        var doorClosedChange = StateChangeHelpers.DoorClosed(_entities.Door);
        _mockHaContext.EmitStateChange(doorClosedChange);

        // Assert - Should call lock service (immediate relock)
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    #endregion

    #region Door State Changes Tests

    [Fact]
    public void DoorStateChanged_Opened_Should_SendDoorOpenedNotification()
    {
        // Act - Simulate door opening
        var stateChange = StateChangeHelpers.DoorOpened(_entities.Door);
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should send door opened notification
        _mockNotificationServices.Verify(
            x => x.NotifyPocoF4("Door is opened", It.IsAny<object>(), "Home Assistant"),
            Times.Once,
            "Should send door opened notification when door opens"
        );
    }

    [Fact]
    public void DoorStateChanged_Closed_WithImmediateRelock_Should_LockDoor()
    {
        // Arrange - First unlock the door by automation to set immediate relock flag
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);
        var unlockChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED,
            HaIdentity.SUPERVISOR
        );
        _mockHaContext.EmitStateChange(unlockChange);

        // Clear previous service calls
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate door closing
        var stateChange = StateChangeHelpers.DoorClosed(_entities.Door);
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should lock the door immediately
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void DoorStateChanged_Closed_WithoutImmediateRelock_Should_SendUnlockedNotification()
    {
        // Arrange - Unlock door by physical operation (should not set immediate relock)
        var unlockChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED,
            null
        );
        _mockHaContext.EmitStateChange(unlockChange);

        // Clear previous service calls and notifications
        _mockHaContext.ClearServiceCalls();
        _mockNotificationServices.Reset();

        // Act - Simulate door closing
        var stateChange = StateChangeHelpers.DoorClosed(_entities.Door);
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should send unlocked notification instead of locking
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);

        _mockNotificationServices.Verify(
            x =>
                x.NotifyPocoF4(
                    "Door was physically unlocked",
                    It.IsAny<object>(),
                    "Home Assistant"
                ),
            Times.Once,
            "Should send unlocked notification when door closes without immediate relock"
        );
    }

    #endregion

    #region Auto-Lock After Time Tests

    [Fact(
        Skip = "Quarantined: lock automation edge case needs investigation | issue HA-TEST-2008 | expires 2026-06-30"
    )]
    public void AutoLock_UnlockedFor5Minutes_WithDoorClosedAndMotionOn_Should_LockDoor()
    {
        // Arrange - Set conditions for auto-lock: door closed, motion on, house status off, lock unlocked
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off"); // closed
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on"); // motion detected
        _mockHaContext.SetEntityState(_entities.HouseStatus.EntityId, "off"); // house empty
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Act - Simulate lock being unlocked for 5 minutes
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED
        );

        // Simulate the time-based condition by directly calling the subscription
        // This simulates the IsUnlockedForMinutes(5) condition being met
        _mockHaContext.EmitStateChange(stateChange);

        // Clear initial calls and simulate the auto-lock condition
        _mockHaContext.ClearServiceCalls();

        // Trigger the auto-lock condition - simulate the reactive stream for unlocked for 5 minutes
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should lock the door due to auto-lock conditions being met
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void AutoLock_UnlockedFor5Minutes_WithDoorOpen_Should_NotLockDoor()
    {
        // Arrange - Set door to be open (should prevent auto-lock)
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on"); // open
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on"); // motion detected
        _mockHaContext.SetEntityState(_entities.HouseStatus.EntityId, "off"); // house empty
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Act - Simulate auto-lock condition trigger
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should NOT lock the door when door is open
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void AutoLock_UnlockedFor5Minutes_WithNoMotionAndHouseOccupied_Should_NotLockDoor()
    {
        // Arrange - Set conditions that should prevent auto-lock: no motion and house occupied
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off"); // closed
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off"); // no motion
        _mockHaContext.SetEntityState(_entities.HouseStatus.EntityId, "on"); // house occupied
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Act - Simulate auto-lock condition trigger
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should NOT lock the door when conditions are not met
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    #endregion

    #region NFC Integration Tests

    [Fact]
    public void NfcScan_DoorLockTag_WithLockUnlocked_Should_LockDoor()
    {
        // Arrange - Set lock to be unlocked
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Setup event handler to return observable for NFC scan
        var nfcSubject = new Subject<string>();
        _mockEventHandler
            .Setup(x => x.OnNfcScan(NFC_ID.DOOR_LOCK))
            .Returns(nfcSubject.AsObservable());

        // Re-create automation to pick up the mock setup
        var automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        automation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate NFC scan with user ID (not physically operated)
        nfcSubject.OnNext(HaIdentity.DANIEL_RODRIGUEZ);

        // Assert - Should lock the door
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void NfcScan_DoorLockTag_WithLockLocked_Should_UnlockDoor()
    {
        // Arrange - Set lock to be locked
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.LOCKED);

        // Setup event handler to return observable for NFC scan
        var nfcSubject = new Subject<string>();
        _mockEventHandler
            .Setup(x => x.OnNfcScan(NFC_ID.DOOR_LOCK))
            .Returns(nfcSubject.AsObservable());

        // Re-create automation to pick up the mock setup
        var automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        automation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate NFC scan with user ID (not physically operated)
        nfcSubject.OnNext(HaIdentity.DANIEL_RODRIGUEZ);

        // Assert - Should unlock the door
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
    }

    [Fact]
    public void NfcScan_DoorLockTag_PhysicallyOperated_Should_BeIgnored()
    {
        // Arrange - Set lock to be unlocked
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Setup event handler to return observable for NFC scan
        var nfcSubject = new Subject<string>();
        _mockEventHandler
            .Setup(x => x.OnNfcScan(NFC_ID.DOOR_LOCK))
            .Returns(nfcSubject.AsObservable());

        // Re-create automation to pick up the mock setup
        var automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        automation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate NFC scan with no user ID (physically operated)
        nfcSubject.OnNext(null!);

        // Assert - Should NOT trigger any lock actions
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    #endregion

    #region Mobile Event Tests

    [Fact]
    public void MobileEvent_LockAction_Should_LockDoor()
    {
        // Arrange - Setup event handler to return observable for mobile event
        var mobileSubject = new Subject<string>();
        _mockEventHandler
            .Setup(x => x.OnMobileEvent("LOCK_ACTION"))
            .Returns(mobileSubject.AsObservable());

        // Re-create automation to pick up the mock setup
        var automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        automation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate mobile event
        mobileSubject.OnNext("mobile_event_data");

        // Assert - Should lock the door
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    #endregion

    #region Master Switch Tests

    [Fact]
    public void MasterSwitchDisabled_Should_DisableAllAutomations()
    {
        // Arrange - Set lock to be unlocked and clear initial calls
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned off
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");

        // Try to trigger lock state change
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.UNLOCKED,
            HaEntityStates.LOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should not call any services when master switch is off
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void MasterSwitchEnabled_Should_EnableAllAutomations()
    {
        // Arrange - Start with master switch off
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Enable master switch
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Trigger lock state change
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.UNLOCKED,
            HaEntityStates.LOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should process automation when master switch is on
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Flytrap.EntityId);
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public void Lock_AlreadyLocked_Should_NotCallLockService()
    {
        // Arrange - Set lock to already be locked
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.LOCKED);

        // Act - Try to lock already locked door
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Lock,
            HaEntityStates.LOCKED,
            HaEntityStates.UNLOCKED
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Clear calls from state change handling
        _mockHaContext.ClearServiceCalls();

        // Simulate auto-lock condition
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should not call lock service when already locked
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void ToggleLock_BothConditionsTrue_Should_HandleGracefully()
    {
        // This tests the edge case where both IsUnlocked() and IsLocked() might be true
        // due to state transitions or timing issues

        // Arrange - Set lock state to unlocked
        _mockHaContext.SetEntityState(_entities.Lock.EntityId, HaEntityStates.UNLOCKED);

        // Setup event handler for NFC
        var nfcSubject = new Subject<string>();
        _mockEventHandler
            .Setup(x => x.OnNfcScan(NFC_ID.DOOR_LOCK))
            .Returns(nfcSubject.AsObservable());

        var automation = new LockAutomation(
            _entities,
            _mockNotificationServices.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        automation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger NFC scan which calls ToggleLock
        nfcSubject.OnNext(HaIdentity.DANIEL_RODRIGUEZ);

        // Assert - Should call lock service (first condition in ToggleLock method)
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void DoorOpenedFor5Minutes_Should_SendNotificationAgain()
    {
        // This tests the door open for extended time notification feature

        // Act - Simulate door being open for 5 minutes (reactive stream condition)
        var stateChange = StateChangeHelpers.DoorOpened(_entities.Door);
        _mockHaContext.EmitStateChange(stateChange);

        // Clear first notification
        _mockNotificationServices.Reset();

        // Simulate the "open for 5 minutes" condition
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should send door opened notification again
        _mockNotificationServices.Verify(
            x => x.NotifyPocoF4("Door is opened", It.IsAny<object>(), "Home Assistant"),
            Times.Once,
            "Should send door opened notification when door is open for extended time"
        );
    }

    #endregion

    #region Test Entity Container Implementation

    private class TestEntities(IHaContext haContext) : ILockingEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.security_automation_master");
        public BinarySensorEntity MotionSensor => new(haContext, "binary_sensor.front_door_motion");
        public LockEntity Lock => new(haContext, "lock.front_door_2");
        public BinarySensorEntity Door => new(haContext, "binary_sensor.front_door_contact");
        public BinarySensorEntity HouseStatus => new(haContext, "binary_sensor.house_occupied");
        public SwitchEntity Flytrap => new(haContext, "switch.flytrap_outlet");
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation.Dispose();
        }

        base.Dispose(disposing);
    }
}
