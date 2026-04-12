using HomeAutomation.apps.Common.Services;
using HomeAutomation.apps.Security.Automations;
using HomeAutomation.apps.Security.Automations.Entities;

namespace HomeAutomation.Tests.Security.Automations;

/// <summary>
/// Comprehensive behavioral tests for AccessControlAutomation covering critical access control functionality
/// Tests home/away triggers, lock automation, door state coordination, and suppression logic
/// </summary>
public class AccessControlAutomationTests : AutomationTestBase<AccessControlAutomation>
{
    private MockHaContext _mockHaContext => HaContext;
    private Mock<ILogger<AccessControlAutomation>> _mockLogger => Logger;
    private readonly TestEntities _entities;
    private readonly Mock<IPersonController> _mockPerson1Controller;
    private readonly Mock<IPersonController> _mockPerson2Controller;
    private readonly AccessControlAutomation _automation;

    // Observable subjects for controlling person controller events in tests
    private readonly Subject<string> _person1ArrivedHome = new();
    private readonly Subject<string> _person1LeftHome = new();
    private readonly Subject<string> _person1DirectUnlock = new();
    private readonly Subject<string> _person2ArrivedHome = new();
    private readonly Subject<string> _person2LeftHome = new();

    public AccessControlAutomationTests()
    {
        // Create test entities wrapper
        _entities = new TestEntities(_mockHaContext);

        // Create mock person controllers
        _mockPerson1Controller = CreateMockPersonController(1);
        _mockPerson2Controller = CreateMockPersonController(2);

        var personControllers = new List<IPersonController>
        {
            _mockPerson1Controller.Object,
            _mockPerson2Controller.Object,
        };

        _automation = new AccessControlAutomation(personControllers, _entities, _mockLogger.Object);

        StartAutomation(_automation);
    }

    #region Home Trigger Tests

    [Fact]
    public void HomeTrigger_PersonNotHome_ShouldSetPersonHome()
    {
        // Act - Trigger Person 1's arrived home via person controller observable
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);

        // Assert - Should call SetHome for Person 1
        _mockPerson1Controller.Verify(
            p => p.SetHome(),
            Times.Once,
            "Should set Person 1 home when home trigger activates"
        );
    }

    [Fact]
    public void HomeTrigger_HouseWasEmpty_ShouldUnlockDoor()
    {
        // Arrange - House is empty, Person 1 is away
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off"); // House empty

        // First trigger house becoming empty
        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 comes home via person controller observable
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);

        // Assert - Should unlock door and set Person 1 home
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);

        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);

        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "off", "on");
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void HomeTrigger_HouseOccupied_ShouldUnlockDoor()
    {
        // Arrange - House is occupied, Person 1 is away, no suppression active
        _mockHaContext.SetEntityState(_entities.House.EntityId, "on"); // House occupied

        // Act - Person 1 comes home via person controller observable
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);

        // Assert - Should unlock door and set Person 1 home
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);

        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);

        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "off", "on");
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void HomeTrigger_DuringSuppressionWindow_ShouldIgnoreUnlock()
    {
        // Arrange - House was empty, Person 1 came home (creating suppression), now Person 2 comes home
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off"); // House empty

        // First trigger house becoming empty
        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        // Person 1 comes home first (should unlock and create suppression)
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on"); // House occupied on 1st arrival

        // Act - Person 2 comes home during suppression window via person controller observable
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should set Person 2 home but NOT unlock door (suppression active)
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    #endregion

    #region Away Trigger Tests

    [Fact]
    public void AwayTrigger_WhenDoorAlreadyClosed_ShouldLockAndSetAway()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);

        // Assert - Should lock immediately when the door is already closed
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
    }

    [Fact]
    public void AwayTrigger_WhenDoorIsOpen_ShouldSetAwayAndWaitForDoorClose()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);

        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    #endregion

    #region Door State Management Tests

    [Fact]
    public void DoorClosed_ShouldSetupSubscriptionForDoorState()
    {
        // Note: Door state management involves reactive streams that are complex to test
        // This test verifies the door closed subscription is working

        // Arrange - Door is open
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        // Act - Door closes
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.EmitStateChange(doorClosedChange);

        // Assert - Door state change is processed (internal flag set)
        // The actual logic verification requires integration with away triggers which have timing delays
        // This test confirms the subscription handles door state changes correctly
        // No service calls expected for door state changes alone
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void DoorClosed_WhenPendingDepartureLockExists_ShouldLockOnce()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "off", "on");
        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void Arrival_ShouldClearPendingDepartureLock()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on");
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);
    }

    #endregion

    #region House State Management Tests

    [Fact]
    public void HouseBecomeEmpty_ShouldSetWasHouseEmptyFlag()
    {
        // Act - House becomes empty
        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);

        // Assert - Should unlock door (proving _wasHouseEmpty was set to true)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);
    }

    [Fact]
    public void HouseBecomeEmpty_ShouldClearSuppressionTimer()
    {
        // Arrange - Create suppression by having Person 1 come home when house was empty
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off"); // House empty

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - House becomes empty again (should clear suppression)
        var houseEmptyAgain = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyAgain);

        // Now Person 2 comes home (should unlock since suppression was cleared)
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock door (proving suppression was cleared)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
    }

    #endregion

    #region Suppression Timer Tests

    [Fact]
    public void SuppressionTimer_DuringWindow_ShouldPreventUnlock()
    {
        // Arrange - House empty, Person 1 comes home (creates suppression)
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on"); // House occupied on 1st arrival

        // Act - Person 2 comes home during suppression window (before 10 minutes)
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Test at various points during suppression window
        _mockHaContext.AdvanceTimeByMinutes(1);
        _mockHaContext.AdvanceTimeByMinutes(5);
        _mockHaContext.AdvanceTimeByMinutes(3); // Total: 9 minutes

        // Assert - Should still be suppressed (no unlock)
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void SuppressionTimer_AfterExpiry_ShouldAllowUnlock()
    {
        // Arrange - House empty, Person 1 comes home (creates 10-minute suppression timer)
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - Advance time by exactly 10 minutes (suppression timer expires)
        _mockHaContext.AdvanceTimeByMinutes(10);

        // Person 2 comes home after suppression expired
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock door (suppression expired)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
    }

    [Fact]
    public void SuppressionTimer_ClearedByHouseEmpty_ShouldDisposeTimer()
    {
        // Arrange - House empty, Person 1 comes home (creates suppression timer)
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - House becomes empty again after 5 minutes (should clear suppression)
        _mockHaContext.AdvanceTimeByMinutes(5);

        var houseEmptyAgain = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyAgain);

        // Person 2 comes home immediately after house empty (suppression should be cleared)
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock door (suppression was cleared by house empty)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
    }

    #endregion

    #region Suppression Logic Tests

    [Fact]
    public void SuppressionTimer_After10Minutes_ShouldAllowUnlockAgain()
    {
        // This test verifies the suppression timer logic, though we can't easily test the actual timer
        // We test the logic flow that would occur when suppression ends

        // Arrange - House was empty, Person 1 comes home (creates suppression)
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        _mockHaContext.AdvanceTimeByMinutes(10);

        // Act - Person 2 comes home after suppression cleared
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock door since suppression was cleared
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
    }

    #endregion

    #region Multiple Person Scenarios

    [Fact]
    public void MultiplePeople_BothAwayTriggers_ShouldSetBothAwayAndLockOnNextDoorClose()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _person2LeftHome.OnNext(_entities.Person2AwayTrigger.EntityId);

        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockPerson2Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockPerson2Controller.Verify(p => p.SetAway(), Times.Once);
    }

    [Fact]
    public void MultiplePeople_BothHomeTriggers_FirstUnlocks_SecondSuppressed()
    {
        // Arrange - House empty, both people away
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 comes home first
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on"); // House occupied on 1st arrival

        // Clear calls from Person 1's unlock
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Person 2 comes home shortly after (during suppression)
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Person 1 should have unlocked, Person 2 should be suppressed
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId); // No unlock calls after suppression
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void AutomationLifecycleManagement_ShouldHandleStartupCorrectly()
    {
        // Note: AccessControlAutomation uses AutomationBase which doesn't have master switch functionality
        // This test verifies the automation starts up and handles subscriptions correctly

        _mockHaContext.ClearServiceCalls();

        // Act - Trigger home arrived (automation should be active)
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);

        // Assert - Should process automation (no master switch to disable it)
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);
    }

    [Fact]
    public void StartAutomation_ShouldSubscribeToPersonTriggersWithoutImmediateReplay()
    {
        _mockPerson1Controller.Verify(
            p => p.OnArrived(It.Is<BinaryDuration?>(d => d != null && d.StartImmediately == false)),
            Times.Once
        );
        _mockPerson1Controller.Verify(
            p =>
                p.OnDeparted(
                    It.Is<BinaryDuration?>(d =>
                        d != null && d.StartImmediately == false && d.Seconds == 0
                    )
                ),
            Times.Once
        );
        _mockPerson1Controller.Verify(
            p =>
                p.OnUnlocked(It.Is<BinaryDuration?>(d => d != null && d.StartImmediately == false)),
            Times.Once
        );
    }

    #endregion

    #region Timing Integration Tests

    [Fact]
    public void TimingRaceCondition_MultiplePeopleWithinSuppression()
    {
        // Test complex scenario: house empty -> person 1 arrives -> person 2 arrives during suppression ->
        // suppression expires -> person 2 leaves -> person 2 returns

        // Arrange - House empty, both people away
        _mockHaContext.SetEntityState(_entities.House.EntityId, "off");

        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.EmitStateChange(houseEmptyChange);
        _mockHaContext.ClearServiceCalls();

        // Person 1 comes home (should unlock and start suppression)
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on"); // House occupied on 1st arrival

        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Person 2 comes home 3 minutes later (during suppression)
        _mockHaContext.AdvanceTimeByMinutes(3);

        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Should be suppressed
        _mockPerson2Controller.Verify(p => p.SetHome(), Times.Once);
        _mockHaContext.ShouldNeverHaveCalledLock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Advance past suppression window (7 more minutes = 10 total)
        _mockHaContext.AdvanceTimeByMinutes(7);

        // Now person 2 leaves while the door is still open, then the door closes.
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");
        _person2LeftHome.OnNext(_entities.Person2AwayTrigger.EntityId);

        _mockPerson2Controller.Verify(p => p.SetAway(), Times.Never);

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockPerson2Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();

        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock (no suppression active)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
    }

    [Fact]
    public void TimingEdgeCase_PendingDepartureLock_ShouldNotExpireBeforeDoorCloses()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "on");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.AdvanceTimeByMinutes(6);

        _mockHaContext.SimulateStateChange(_entities.Door.EntityId, "on", "off");

        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void AwayTrigger_SubscriptionHandlesObservableEvents()
    {
        _mockHaContext.SetEntityState(_entities.Door.EntityId, "off");

        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);

        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
    }

    [Fact]
    public void AwayTrigger_StateChangeSubscription_ShouldNotTriggerWithoutPersonControllerEvent()
    {
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.EmitStateChange(awayTriggerChange);

        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void HomeTrigger_TurnedOff_ShouldIgnore()
    {
        // Act - Trigger turning OFF (should be ignored by IsOn() filter)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1HomeTrigger,
            "on",
            "off"
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should ignore off state changes
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void AwayTrigger_TurnedOn_ShouldIgnore()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.EmitStateChange(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Away trigger turning ON (should be ignored by IsOffForSeconds filter)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "off",
            "on"
        );
        _mockHaContext.EmitStateChange(stateChange);

        // Assert - Should ignore on state changes
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    #endregion

    [Fact]
    public void DirectUnlockTriggers_ShouldUnlockDoor()
    {
        // Act - Trigger direct unlock via person controller observable
        _person1DirectUnlock.OnNext(_entities.Person1DirectUnlockTrigger.EntityId);

        // Assert - Should unlock door immediately
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(
            p => p.SetHome(),
            Times.Once,
            "Should set Person 1 home when home trigger activates"
        );
    }

    #region Helper Methods

    private Mock<IPersonController> CreateMockPersonController(int index)
    {
        var mock = new Mock<IPersonController>();
        mock.SetupGet(p => p.Name).Returns($"Person{index}");

        // Setup observables based on person number
        if (index == 1)
        {
            mock.Setup(p => p.OnArrived(It.IsAny<BinaryDuration?>())).Returns(_person1ArrivedHome);
            mock.Setup(p => p.OnDeparted(It.IsAny<BinaryDuration?>())).Returns(_person1LeftHome);
            mock.Setup(p => p.OnUnlocked(It.IsAny<BinaryDuration?>()))
                .Returns(_person1DirectUnlock);
        }
        else
        {
            mock.Setup(p => p.OnArrived(It.IsAny<BinaryDuration?>())).Returns(_person2ArrivedHome);
            mock.Setup(p => p.OnDeparted(It.IsAny<BinaryDuration?>())).Returns(_person2LeftHome);
            mock.Setup(p => p.OnUnlocked(It.IsAny<BinaryDuration?>()))
                .Returns(Observable.Never<string>()); // Person 2 has no direct unlock
        }

        return mock;
    }

    #endregion

    #region Test Entity Container Implementation

    private class TestEntities(IHaContext haContext) : IAccessControlAutomationEntities
    {
        public BinarySensorEntity Door => new(haContext, "binary_sensor.front_door_contact");
        public BinarySensorEntity House => new(haContext, "binary_sensor.house_occupied");
        public LockEntity Lock => new(haContext, "lock.front_door");

        // Test trigger entities for testing person controllers
        public BinarySensorEntity Person1HomeTrigger =>
            new(haContext, "binary_sensor.person1_home_trigger");
        public BinarySensorEntity Person1AwayTrigger =>
            new(haContext, "binary_sensor.person1_away_trigger");
        public BinarySensorEntity Person1DirectUnlockTrigger =>
            new(haContext, "binary_sensor.person1_direct_unlock_trigger");
        public BinarySensorEntity Person2HomeTrigger =>
            new(haContext, "binary_sensor.person2_home_trigger");
        public BinarySensorEntity Person2AwayTrigger =>
            new(haContext, "binary_sensor.person2_away_trigger");
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation.Dispose();

            // Dispose observable subjects
            _person1ArrivedHome.Dispose();
            _person1LeftHome.Dispose();
            _person1DirectUnlock.Dispose();
            _person2ArrivedHome.Dispose();
            _person2LeftHome.Dispose();

            // Reset scheduler to default
            SchedulerProvider.Reset();
        }

        base.Dispose(disposing);
    }
}
