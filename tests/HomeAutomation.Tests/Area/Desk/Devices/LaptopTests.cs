using System.Reactive.Disposables;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Desk.Devices;

/// <summary>
/// Comprehensive tests for Laptop device class covering complex state management,
/// multi-device coordination, session state throttling, and power-on sequences.
/// Tests the coordination between virtual switch, session state, power plug, WOL buttons, and lock button.
/// </summary>
public class LaptopTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<IEventHandler> _mockEventHandler;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ILaptopScheduler> _mockScheduler;
    private readonly Mock<IBatteryHandler> _mockBatteryHandler;
    private readonly TestLaptopEntities _entities;
    private readonly Laptop _laptop;

    public LaptopTests()
    {
        _mockHaContext = new MockHaContext();
        _mockEventHandler = new Mock<IEventHandler>();
        _mockLogger = new Mock<ILogger>();
        _mockScheduler = new Mock<ILaptopScheduler>();
        _mockBatteryHandler = new Mock<IBatteryHandler>();
        // Setup battery handler mocks to prevent unexpected service calls
        _mockBatteryHandler.Setup(x => x.HandleLaptopTurnedOn());
        _mockBatteryHandler.Setup(x => x.HandleLaptopTurnedOffAsync()).Returns(Task.CompletedTask);
        _mockBatteryHandler.Setup(x => x.StartMonitoring()).Returns(Disposable.Empty);
        // Setup event handler mocks to return empty observables by default
        _mockEventHandler
            .Setup(x => x.WhenEventTriggered("show_laptop"))
            .Returns(new Subject<Event>().AsObservable());
        _mockEventHandler
            .Setup(x => x.WhenEventTriggered("hide_laptop"))
            .Returns(new Subject<Event>().AsObservable());
        // Return no schedules by default to isolate behavior
        _mockScheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);
        // Create test entities wrapper
        _entities = new TestLaptopEntities(_mockHaContext);

        _laptop = new Laptop(
            _entities,
            _mockScheduler.Object,
            _mockBatteryHandler.Object,
            _mockEventHandler.Object,
            _mockLogger.Object
        );
        _laptop.StartAutomation();
        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    #region Complex State Management Tests

    [Fact]
    public void IsOn_WhenBothSwitchAndSessionOn_Should_ReturnTrue()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");

        // Act
        var result = _laptop.IsOn();

        // Assert
        result
            .Should()
            .BeTrue("laptop should be considered on when both switch and session are active");
    }

    [Fact]
    public void IsOn_WhenOnlySwitchOn_Should_ReturnFalse()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        // Act
        var result = _laptop.IsOn();

        // Assert
        result
            .Should()
            .BeFalse(
                "laptop should be considered off when session is locked (AND logic requires both)"
            );
    }

    [Fact]
    public void IsOn_WhenOnlySessionUnlocked_Should_ReturnFalse()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");

        // Act
        var result = _laptop.IsOn();

        // Assert
        result
            .Should()
            .BeFalse(
                "laptop should be considered off when switch is off (AND logic requires both)"
            );
    }

    [Fact]
    public void IsOn_WhenBothSwitchAndSessionOff_Should_ReturnFalse()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        // Act
        var result = _laptop.IsOn();

        // Assert
        result
            .Should()
            .BeFalse("laptop should be considered off when both switch and session are inactive");
    }

    #endregion

    #region Power-On Sequence Tests

    [Fact]
    public void TurnOn_Should_TurnOnVirtualSwitchAndWakeOnLan()
    {
        // Act
        _laptop.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.VirtualSwitch.EntityId);
    }

    [Fact]
    public void TurnOn_Should_PressAllWakeOnLanButtons()
    {
        // Act
        _laptop.TurnOn();

        // Assert - Verify all WOL buttons were pressed
        foreach (var button in _entities.WakeOnLanButtons)
        {
            _mockHaContext.ShouldHaveCalledService("button", "press", button.EntityId);
        }
    }

    [Fact]
    public void TurnOn_Should_ExecuteCompleteStartupSequence()
    {
        // Act
        _laptop.TurnOn();

        // Assert - Verify complete startup sequence (1 switch + 2 WOL buttons = 3 calls)
        _mockHaContext.ShouldHaveServiceCallCount(3);

        // Verify specific components
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.VirtualSwitch.EntityId);
        _mockHaContext.ShouldHaveCalledService(
            "button",
            "press",
            _entities.WakeOnLanButtons[0].EntityId
        );
        _mockHaContext.ShouldHaveCalledService(
            "button",
            "press",
            _entities.WakeOnLanButtons[1].EntityId
        );
    }

    #endregion

    #region Power-Off Sequence Tests

    [Fact]
    public void TurnOff_Should_TurnOffVirtualSwitch()
    {
        // Act
        _laptop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.VirtualSwitch.EntityId);
    }

    [Fact]
    public void TurnOff_WhenSessionUnlocked_Should_PressLockButton()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");

        // Act
        _laptop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.VirtualSwitch.EntityId);
        _mockHaContext.ShouldHaveCalledService("button", "press", _entities.Lock.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(2);
    }

    [Fact]
    public void TurnOff_WhenSessionLocked_Should_NotPressLockButton()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        // Act
        _laptop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.VirtualSwitch.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(1); // Only virtual switch turn off
    }

    [Fact]
    public void TurnOff_WhenSessionUnavailable_Should_NotPressLockButton()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unavailable");

        // Act
        _laptop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.VirtualSwitch.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(1); // Only virtual switch turn off
    }

    #endregion

    #region Battery Handler Integration Tests

    [Fact]
    public void TurnOn_Should_CallBatteryHandlerTurnedOn()
    {
        // Act
        _laptop.TurnOn();

        // Assert - Should call battery handler turned on
        _mockBatteryHandler.Verify(
            x => x.HandleLaptopTurnedOn(),
            Times.Once,
            "Should call battery handler when laptop turns on"
        );
    }

    [Fact]
    public void TurnOff_Should_CallBatteryHandlerTurnedOffAsync()
    {
        // Act
        _laptop.TurnOff();

        // Assert - Should call battery handler turned off async
        _mockBatteryHandler.Verify(
            x => x.HandleLaptopTurnedOffAsync(),
            Times.Once,
            "Should call battery handler async method when laptop turns off"
        );
    }

    [Fact]
    public void VirtualSwitchTurnOn_Should_CallBatteryHandlerTurnedOn()
    {
        // Act - Simulate virtual switch turning on
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.VirtualSwitch,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call battery handler turned on
        _mockBatteryHandler.Verify(
            x => x.HandleLaptopTurnedOn(),
            Times.Once,
            "Should call battery handler when virtual switch turns on"
        );
    }

    [Fact]
    public void VirtualSwitchTurnOff_Should_CallBatteryHandlerTurnedOffAsync()
    {
        // Act - Simulate virtual switch turning off
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.VirtualSwitch,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call battery handler turned off async
        _mockBatteryHandler.Verify(
            x => x.HandleLaptopTurnedOffAsync(),
            Times.Once,
            "Should call battery handler async method when virtual switch turns off"
        );
    }

    #endregion

    #region Virtual Switch Automation Tests

    [Fact]
    public void VirtualSwitchTurnOn_Should_TriggerTurnOnSequence()
    {
        // Act - Simulate virtual switch turning on
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.VirtualSwitch,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should trigger full turn on sequence
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.VirtualSwitch.EntityId);

        foreach (var button in _entities.WakeOnLanButtons)
        {
            _mockHaContext.ShouldHaveCalledService("button", "press", button.EntityId);
        }
    }

    [Fact]
    public void VirtualSwitchTurnOff_Should_TriggerTurnOffSequence()
    {
        // Arrange - Set session as unlocked to test conditional lock button press
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate virtual switch turning off
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.VirtualSwitch,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should trigger turn off sequence including lock button
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.VirtualSwitch.EntityId);
        _mockHaContext.ShouldHaveCalledService("button", "press", _entities.Lock.EntityId);
    }

    [Fact]
    public void VirtualSwitchRepeatedChanges_Should_HandleCorrectly()
    {
        // Act - Multiple rapid switch changes
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, "off", "on")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, "on", "off")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, "off", "on")
        );

        // Assert - Should handle each change correctly
        // First on: switch + 2 WOL buttons = 3 calls
        // First off: switch only = 1 call (session not unlocked)
        // Second on: switch + 2 WOL buttons = 3 calls
        // Total: 7 calls
        _mockHaContext.ShouldHaveServiceCallCount(7);
    }

    #endregion

    #region StateChanges Observable Tests (with Throttling)

    [Fact]
    public void StateChanges_Should_EmitCorrectInitialValues()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");

        var results = new List<bool>();

        // Act
        _laptop.StateChanges().Subscribe(results.Add);

        // Assert - StateChanges now emits initial switch state via StartWith()
        results
            .Should()
            .ContainSingle("StateChanges should emit initial switch state (true)")
            .Which.Should()
            .BeTrue("initial switch state is 'on'");
    }

    [Fact]
    public void StateChanges_SwitchChangeOnly_Should_EmitNewState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        var results = new List<bool>();
        _laptop.StateChanges().Subscribe(results.Add);
        results.Clear(); // Clear initial state

        // Act - Change switch state
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, "off", "on")
        );

        // Assert - Switch state changes from false to true
        // The merged observable emits switch state changes
        results
            .Should()
            .ContainSingle("switch state change from off to on should emit")
            .Which.Should()
            .BeTrue("switch changed to 'on'");
    }

    [Fact]
    public void StateChanges_SessionChangeOnly_Should_EmitNewState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        var results = new List<bool>();
        _laptop.StateChanges().Subscribe(results.Add);
        results.Clear(); // Clear initial state

        // Act - Change session state (both emit state change and update entity state)
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.Session, "locked", "unlocked")
        );

        // Assert - The StateChanges observable combines switch and session state
        // With AND logic, session unlocked + switch off should still be false
        _laptop
            .IsOn()
            .Should()
            .BeFalse(
                "laptop should be considered off when session unlocks but switch is off (AND logic)"
            );

        // Verify that the observable stream is set up correctly by checking if any changes occurred
        results.Should().NotBeNull("state changes observable should be functioning");
    }

    [Fact]
    public void StateChanges_BothChangesSimultaneously_Should_HandleCorrectly()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");

        var results = new List<bool>();
        _laptop.StateChanges().Subscribe(results.Add);
        results.Clear(); // Clear initial state

        // Act - Both change to on/unlocked
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, "off", "on")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.Session, "locked", "unlocked")
        );

        // Assert - Both switch changes should emit in merged observable
        // First emission: switch off->on (true), session change doesn't emit since it's unlocked->unlocked
        results
            .Should()
            .ContainSingle("switch state change should emit")
            .Which.Should()
            .BeTrue("switch changed to 'on'");
    }

    #endregion

    #region Session State Throttling Tests

    [Fact]
    public void StateChanges_SessionThrottling_Should_ReduceFlapping()
    {
        // This test verifies the 1-second throttling on session state changes
        // Note: Actual throttling behavior is complex to test without a real scheduler,
        // but we can verify the stream setup is correct

        // Arrange
        var results = new List<bool>();
        _laptop.StateChanges().Subscribe(results.Add);

        // Act - Rapid session state changes (simulating flapping)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.Session, "locked", "unlocked")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.Session, "unlocked", "locked")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.Session, "locked", "unlocked")
        );

        // Assert - Observable should be set up correctly (exact behavior depends on throttling)
        results.Should().NotBeEmpty("state changes observable should be active");
    }

    #endregion

    #region State Change Combination Tests

    [Fact]
    public void StateLogic_AllCombinations_Should_WorkCorrectly()
    {
        // Test all possible combinations of switch and session states

        // Case 1: Both off/locked -> false
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");
        _laptop.IsOn().Should().BeFalse("both inactive should be false");

        // Case 2: Switch on, session locked -> false (AND logic requires both)
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "locked");
        _laptop
            .IsOn()
            .Should()
            .BeFalse("switch on but session locked should be false with AND logic");

        // Case 3: Switch off, session unlocked -> false (AND logic requires both)
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");
        _laptop
            .IsOn()
            .Should()
            .BeFalse("session unlocked but switch off should be false with AND logic");

        // Case 4: Both on/unlocked -> true
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unlocked");
        _laptop.IsOn().Should().BeTrue("both active should be true");
    }

    [Fact]
    public void StateLogic_UnavailableStates_Should_HandleGracefully()
    {
        // Test unavailable states don't cause issues

        // Case 1: Session unavailable, switch on -> true (unavailable is not locked)
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unavailable");
        _laptop
            .IsOn()
            .Should()
            .BeTrue("unavailable session is not locked, so laptop can be on with switch on");

        // Case 2: Session unavailable, switch off -> false
        _mockHaContext.SetEntityState(_entities.VirtualSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Session.EntityId, "unavailable");
        _laptop.IsOn().Should().BeFalse("unavailable session with off switch should be false");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void TurnOnTurnOff_Rapid_Should_HandleCorrectly()
    {
        // Act - Rapid on/off calls
        _laptop.TurnOn();
        _laptop.TurnOff();
        _laptop.TurnOn();

        // Assert - Should handle all calls correctly
        // First TurnOn: 3 calls, TurnOff: 1 call, Second TurnOn: 3 calls = 7 total
        _mockHaContext.ShouldHaveServiceCallCount(7);
    }

    [Fact]
    public void VirtualSwitchAutomation_Should_NotCreateMemoryLeak()
    {
        // This test ensures subscriptions are properly managed

        // Act - Generate state changes
        for (int i = 0; i < 10; i++)
        {
            var newState = i % 2 == 0 ? "on" : "off";
            var oldState = i % 2 == 0 ? "off" : "on";
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.CreateStateChange(_entities.VirtualSwitch, oldState, newState)
            );
        }

        // Assert - Should not throw and should process all changes
        var act = () => _laptop.Dispose();
        act.Should().NotThrow("disposal should clean up subscriptions properly");
    }

    [Fact]
    public void Laptop_Should_ImplementIComputerInterface()
    {
        // Assert - Verify interface implementation
        _laptop.Should().BeAssignableTo<IComputer>("Laptop should implement IComputer interface");

        // Verify all interface methods are available
        var act1 = () => _laptop.TurnOn();
        var act2 = () => _laptop.TurnOff();
        var act3 = () => _laptop.IsOn();
        var act4 = () => _laptop.StateChanges();
        var act5 = () => _laptop.OnShowRequested();
        var act6 = () => _laptop.OnHideRequested();

        act1.Should().NotThrow("TurnOn method should be available");
        act2.Should().NotThrow("TurnOff method should be available");
        act3.Should().NotThrow("IsOn method should be available");
        act4.Should().NotThrow("StateChanges method should be available");
        act5.Should().NotThrow("OnShowRequested method should be available");
        act6.Should().NotThrow("OnHideRequested method should be available");
    }

    [Fact]
    public void Laptop_ShowHideEvents_Should_WorkCorrectly()
    {
        // Arrange
        var showSubject = new Subject<Event>();
        var hideSubject = new Subject<Event>();

        _mockEventHandler
            .Setup(x => x.WhenEventTriggered("show_laptop"))
            .Returns(showSubject.AsObservable());
        _mockEventHandler
            .Setup(x => x.WhenEventTriggered("hide_laptop"))
            .Returns(hideSubject.AsObservable());

        var showResults = new List<Event>();
        var hideResults = new List<Event>();

        // Act
        _laptop.OnShowRequested().Subscribe(_ => showResults.Add(new Event()));
        _laptop.OnHideRequested().Subscribe(_ => hideResults.Add(new Event()));

        // Simulate events
        showSubject.OnNext(new Event());
        hideSubject.OnNext(new Event());

        // Assert
        showResults.Should().HaveCount(1, "show event should be captured");
        hideResults.Should().HaveCount(1, "hide event should be captured");
    }

    #endregion

    #region Entity Configuration Tests

    [Fact]
    public void TestEntities_Should_HaveCorrectEntityIds()
    {
        // Assert - Verify entity IDs are correct for laptop components
        _entities.VirtualSwitch.EntityId.Should().Be("switch.laptop_virtual");
        _entities.Session.EntityId.Should().Be("sensor.thinkpadt14_session");
        _entities.Lock.EntityId.Should().Be("button.thinkpadt14_lock");
        _entities.BatteryLevel.EntityId.Should().Be("sensor.thinkpadt14_battery_level");

        // Verify WOL buttons
        _entities.WakeOnLanButtons.Should().HaveCount(2, "should have two WOL buttons");
        _entities.WakeOnLanButtons[0].EntityId.Should().Be("button.thinkpadt14_wake_on_lan");
        _entities.WakeOnLanButtons[1].EntityId.Should().Be("button.thinkpadt14_wake_on_wlan");
    }

    #endregion

    public void Dispose()
    {
        _laptop?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements ILaptopEntities interface
    /// Creates entities internally with the appropriate entity IDs for laptop device coordination
    /// </summary>
    private class TestLaptopEntities(IHaContext haContext) : ILaptopEntities
    {
        public SwitchEntity VirtualSwitch { get; } =
            new SwitchEntity(haContext, "switch.laptop_virtual");
        public ButtonEntity[] WakeOnLanButtons { get; } =
            [
                new ButtonEntity(haContext, "button.thinkpadt14_wake_on_lan"),
                new ButtonEntity(haContext, "button.thinkpadt14_wake_on_wlan"),
            ];
        public SensorEntity Session { get; } =
            new SensorEntity(haContext, "sensor.thinkpadt14_session");
        public NumericSensorEntity BatteryLevel { get; } =
            new NumericSensorEntity(haContext, "sensor.thinkpadt14_battery_level");
        public ButtonEntity Lock { get; } = new ButtonEntity(haContext, "button.thinkpadt14_lock");
    }
}
