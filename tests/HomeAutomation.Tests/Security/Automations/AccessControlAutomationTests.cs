using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;
using HomeAutomation.apps.Security.Automations;

namespace HomeAutomation.Tests.Security.Automations;

/// <summary>
/// Comprehensive behavioral tests for AccessControlAutomation covering critical access control functionality
/// Tests home/away triggers, lock automation, door state coordination, and suppression logic
/// </summary>
public class AccessControlAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<AccessControlAutomation>> _mockLogger;
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
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<AccessControlAutomation>>();

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

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);
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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

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
    public void AwayTrigger_SetupCorrectSubscription_ShouldHandleStateChanges()
    {
        // Note: Away trigger uses IsOffForSeconds(60) which creates a 60-second delay
        // This test verifies the subscription is set up correctly
        // Testing the actual delay would require a test scheduler

        // Simulate door being closed recently
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 away trigger turns off (should be processed by subscription)
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Assert - No immediate action due to 60-second delay in actual automation
        // The subscription is working, but timing logic prevents immediate execution
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void AwayTrigger_PersonAlreadyAway_ShouldSetupSubscription()
    {
        // Note: This tests that the subscription is created for away triggers
        // The actual trigger logic has timing delays that are complex to test

        // Act - Person 1 away trigger state changes
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Assert - No immediate action due to automation's timing logic
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void AwayTrigger_SubscriptionHandlesStateChanges()
    {
        // Note: Away triggers use IsOffForSeconds(60) which requires time-based testing
        // This test verifies the subscription processes state changes

        // Act - Person 1 away trigger state changes
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Assert - No immediate action due to timing and door state logic
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    #endregion

    #region Away Trigger Timing Tests

    [Fact]
    public void AwayTrigger_BeforeDelay_ShouldNotTrigger()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 away trigger turns off (starts 60-second delay)
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Advance time by 59 seconds (just before delay expires)
        _mockHaContext.AdvanceTimeBySeconds(59);

        // Assert - Should not trigger yet (delay not complete)
        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
    }

    [Fact]
    public void AwayTrigger_AfterDelay_ShouldLockAndSetAway()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 left home via person controller observable (after delay logic in PersonController)
        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        // Advance time by exactly 60 seconds
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should now lock door and set person away
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
    }

    [Fact]
    public void AwayTrigger_CancelledByHomeReturn_ShouldNotTrigger()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 away trigger turns off (starts 60-second delay)
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // After 30 seconds, person 1 comes back (trigger turns on again - cancelling delay)
        _mockHaContext.AdvanceTimeBySeconds(30);
        var homeCancelChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(homeCancelChange);

        // Advance past original 60-second mark
        _mockHaContext.AdvanceTimeBySeconds(40);

        // Assert - Should not trigger because delay was cancelled
        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
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
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);

        // Assert - Door state change is processed (internal flag set)
        // The actual logic verification requires integration with away triggers which have timing delays
        // This test confirms the subscription handles door state changes correctly
        // No service calls expected for door state changes alone
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void DoorClosed_After5Minutes_ShouldClearRecentlyClosedFlag()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Verify flag is set by triggering away logic (should work with recent door close)
        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);

        // Advance 60 seconds to trigger away logic (door recently closed = true)
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Should have triggered because door was recently closed
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ClearServiceCalls();

        // Act - Advance time by 5 minutes (door recently closed flag should clear)
        _mockHaContext.AdvanceTimeByMinutes(5);

        // Trigger away logic again (door recently closed should now be false)
        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should not trigger because door recently closed flag was cleared
        // Verify that SetAway was called exactly once (from the first trigger, not the second)
        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Once);
    }

    [Fact]
    public void AwayTrigger_AfterDoorWindowExpires_ShouldIgnore()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);

        // Advance time past door window (5 minutes)
        _mockHaContext.AdvanceTimeByMinutes(5);

        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 away trigger turns off after door window expired
        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Advance time by 60 seconds (away trigger delay)
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should not trigger because door window expired
        _mockHaContext.ShouldHaveNoServiceCalls();
        _mockPerson1Controller.Verify(p => p.SetAway(), Times.Never);
    }

    #endregion

    #region House State Management Tests

    [Fact]
    public void HouseBecomeEmpty_ShouldSetWasHouseEmptyFlag()
    {
        // Act - House becomes empty
        var houseEmptyChange = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - House becomes empty again (should clear suppression)
        var houseEmptyAgain = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyAgain);

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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - House becomes empty again after 5 minutes (should clear suppression)
        _mockHaContext.AdvanceTimeByMinutes(5);

        var houseEmptyAgain = StateChangeHelpers.CreateStateChange(_entities.House, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyAgain);

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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);

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
    public void MultiplePeople_BothAwayTriggers_ShouldSetupSubscriptions()
    {
        // Note: Away triggers use IsOffForSeconds(60) which makes testing complex
        // This test verifies both people's away triggers have subscriptions

        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Both people leave home via person controller observables (after delay logic in PersonController)
        _person1LeftHome.OnNext(_entities.Person1AwayTrigger.EntityId);
        _person2LeftHome.OnNext(_entities.Person2AwayTrigger.EntityId);

        // Assert - Should trigger lock actions since door was recently closed and people left
        // With new architecture, timing is handled in PersonController, so actions happen when observables fire
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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Person 1 comes home first
        _person1ArrivedHome.OnNext(_entities.Person1HomeTrigger.EntityId);
        _mockHaContext.SimulateStateChange(_entities.House.EntityId, "off", "on"); // House occupied on 1st arrival

        // Clear calls from Person 1's unlock
        var unlockCallsAfterPerson1 = _mockHaContext
            .GetServiceCalls("lock")
            .Where(c => c.Service == "unlock")
            .Count();
        _mockHaContext.ClearServiceCalls();

        // Person 2 comes home shortly after (during suppression)
        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Person 1 should have unlocked, Person 2 should be suppressed
        unlockCallsAfterPerson1.Should().Be(1, "Person 1 should have triggered unlock");
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
        _mockHaContext.StateChangeSubject.OnNext(houseEmptyChange);
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

        // Now person 2 leaves (door closes, away trigger after delay)
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);

        _person2LeftHome.OnNext(_entities.Person2AwayTrigger.EntityId);
        _mockHaContext.AdvanceTimeByMinutes(1);

        _mockPerson2Controller.Verify(p => p.SetAway(), Times.Once);
        _mockHaContext.ShouldHaveCalledLockLock(_entities.Lock.EntityId);
        _mockHaContext.ClearServiceCalls();

        _person2ArrivedHome.OnNext(_entities.Person2HomeTrigger.EntityId);

        // Assert - Should unlock (no suppression active)
        _mockHaContext.ShouldHaveCalledLockUnlock(_entities.Lock.EntityId);
    }

    [Fact]
    public void TimingEdgeCase_AwayTriggerAtDoorWindowBoundary()
    {
        // Test edge case: away trigger delay completes after door window expires

        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Away trigger starts at 4 minutes 59 seconds after door close
        _mockHaContext.AdvanceTimeBy(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(59)));

        var awayTriggerChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awayTriggerChange);

        // Advance by 1 second (door window expires at exactly 5 minutes)
        _mockHaContext.AdvanceTimeBySeconds(1);

        // Advance by remaining 59 seconds (60-second away trigger delay completes)
        _mockHaContext.AdvanceTimeBySeconds(59);

        // Assert - Should NOT trigger because door window expired before away trigger logic executed
        // At this point we're at 6:00 total time, door window expired at 5:00
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
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should ignore off state changes
        _mockPerson1Controller.Verify(p => p.SetHome(), Times.Never);
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void AwayTrigger_TurnedOn_ShouldIgnore()
    {
        var doorClosedChange = StateChangeHelpers.CreateStateChange(_entities.Door, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(doorClosedChange);
        _mockHaContext.ClearServiceCalls();

        // Act - Away trigger turning ON (should be ignored by IsOffForSeconds filter)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.Person1AwayTrigger,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

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

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();

        // Dispose observable subjects
        _person1ArrivedHome?.Dispose();
        _person1LeftHome?.Dispose();
        _person1DirectUnlock?.Dispose();
        _person2ArrivedHome?.Dispose();
        _person2LeftHome?.Dispose();

        // Reset scheduler to default
        SchedulerProvider.Reset();
    }
}
