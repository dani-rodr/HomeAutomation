using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.Pantry.Automations;

/// <summary>
/// Comprehensive behavioral tests for Pantry MotionAutomation using clean assertion syntax
/// Verifies actual automation behavior with enhanced readability and time-dependent testing
/// </summary>
public class LightAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<LightAutomation>> _mockLogger;
    private readonly TestEntities _entities;
    private readonly LightAutomation _automation;

    public LightAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LightAutomation>>();

        // Create test entities wrapper - much simpler!
        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(_entities, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void MotionDetected_Should_TurnOnPantryLight()
    {
        // Act - Simulate motion sensor turning on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Clean, one-line assertion using entity ID
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MotionCleared_Should_TurnOffBothLights()
    {
        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Clean helper for common pattern using entity IDs
        _mockHaContext.ShouldHaveCalledBothLightsTurnOff(
            _entities.Light.EntityId,
            _entities.MirrorLight.EntityId
        );
    }

    [Fact]
    public void MiScalePresenceDetected_Should_TurnOnMirrorLight()
    {
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");

        // Act - Simulate MiScale presence sensor turning on
        var stateChange = StateChangeHelpers.PresenceDetected(_entities.MiScalePresenceSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Clean syntax with negative assertions using entity IDs
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.MirrorLight.EntityId);
        _mockHaContext.ShouldNeverHaveCalledLight(_entities.Light.EntityId);
    }

    [Fact]
    public void RoomDoorClosed_Should_TurnOnMasterSwitch()
    {
        // Act - Simulate room door closing (IsOff means closed)
        var stateChange = StateChangeHelpers.DoorClosed(_entities.BedroomDoor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Switch assertions work too using entity ID
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOn_Should_TurnOnLight()
    {
        // Arrange - Set motion sensor to be "on" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on (should trigger ControlLightOnMotionChange)
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn on light because motion sensor is already on
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOff_Should_TurnOffLight()
    {
        // Arrange - Set motion sensor to be "off"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn off light because motion sensor is off
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void ComplexScenario_With_ExactCountVerification()
    {
        // Act - First motion is detected, then MiScale presence
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");
        _mockHaContext.SimulateStateChange(_entities.MiScalePresenceSensor.EntityId, "off", "on");

        // Assert - Verify both lights and exact counts using entity IDs
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.MirrorLight.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(3);
    }

    [Fact]
    public void NoMotion_Should_MakeNoServiceCalls()
    {
        // Act - Do nothing (no state changes)

        // Assert - Clean negative assertion
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void MultipleMotionEvents_Should_HandleCorrectSequence()
    {
        // Arrange - Set bathroom sensor to off initially so CombineLatest will trigger turn_off
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");

        // Act - Motion on, off, on again
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        // Trigger bathroom sensor state change to activate CombineLatest when both are off
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.BathroomMotionSensor)
        );

        _mockHaContext.AdvanceTimeBySeconds(60);
        // Continue with final motion detection
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Assert - Verify exact call counts for complex scenarios using entity IDs
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.MirrorLight.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_on",
            2
        );
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            1
        );
        // Note: Service call count may include sensor delay adjustments, so we focus on specific behavior verification
    }

    [Fact]
    public void StateTracking_Should_Work_Correctly()
    {
        // This test verifies our MockHaContext state tracking fix works

        // Arrange - Set initial state
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");

        // Verify initial state
        var initialState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        initialState?.State.Should().Be("off");

        // Act - Simulate state change
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");

        // Assert - State should be updated
        var newState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        newState?.State.Should().Be("on");

        // Verify entity IsOn() works correctly
        _entities
            .MotionSensor.IsOccupied()
            .Should()
            .BeTrue("motion sensor should report occupied after state change");
    }

    [Fact]
    public void Automation_Should_NotThrow_WhenStateChangesOccur()
    {
        // This test ensures automation setup doesn't throw exceptions

        // Act & Assert - Should not throw
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionDetected(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionCleared(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.PresenceDetected(_entities.MiScalePresenceSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.DoorClosed(_entities.BedroomDoor)
            );
        };

        act.Should().NotThrow();
    }

    #region Bathroom Automation Tests

    [Fact]
    public void PantryMotionDetected_Should_TurnOnBathroomAutomation()
    {
        // Arrange - Set initial state for master switch, these automations should be persistent
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");

        // Assert - Should turn on bathroom automation
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);
    }

    [Fact]
    public void BothSensorsOff_Should_TurnOffBathroomAutomation_After1Minute()
    {
        // Arrange - Set both sensors to off state, initial state for master switch, these automations should be persistent
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger state change for pantry sensor (both are now off)
        var pantryStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(pantryStateChange);

        // Also trigger bathroom sensor state change to ensure CombineLatest works
        var bathroomStateChange = StateChangeHelpers.MotionCleared(_entities.BathroomMotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(bathroomStateChange);

        // Assert - Should NOT turn off immediately (30 seconds delay)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should turn off bathroom automation after delay
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);
    }

    [Fact]
    public void OnlyPantryOff_BathroomOn_Should_KeepBathroomAutomationOn()
    {
        // Arrange - Set pantry off, bathroom on
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger state changes
        var pantryStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(pantryStateChange);

        var bathroomStateChange = StateChangeHelpers.MotionDetected(_entities.BathroomMotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(bathroomStateChange);

        // Assert - Should NOT turn off bathroom automation (bathroom sensor still on)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    [Fact]
    public void OnlyBathroomOff_PantryOn_Should_KeepBathroomAutomationOn()
    {
        // Arrange - Set pantry on, bathroom off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger state changes
        var pantryStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(pantryStateChange);

        var bathroomStateChange = StateChangeHelpers.MotionCleared(_entities.BathroomMotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(bathroomStateChange);

        // Assert - Should NOT turn off bathroom automation (pantry sensor still on)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    [Fact]
    public void InitialState_BothSensorsOn_Should_NotTurnOffBathroomAutomation()
    {
        // Arrange - Set both sensors to on initially
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger any state change to activate CombineLatest logic
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn on bathroom automation (motion detected) but never turn off (both sensors on)
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    [Fact]
    public void RapidStateChanges_Should_HandleCorrectly()
    {
        // Act - Rapid sequence: pantry on, off, bathroom on, both off
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.BathroomMotionSensor)
        );

        // Clear calls to focus on final state
        _mockHaContext.ClearServiceCalls();

        // Final state: both sensors off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.BathroomMotionSensor)
        );

        // Assert - Should NOT turn off immediately (30 seconds delay)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should turn off bathroom automation after delay
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);
    }

    [Fact]
    public void ConcurrentSensorChanges_Should_TurnOffBathroomAutomation()
    {
        // Arrange - Set initial states
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate both sensors turning off "simultaneously"
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "off");
        _mockHaContext.SimulateStateChange(_entities.BathroomMotionSensor.EntityId, "on", "off");

        // Assert - Should NOT turn off immediately (30 seconds delay)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should turn off bathroom automation after delay
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);
    }

    [Fact]
    public void StateChangesWithCurrent_Should_ConsiderInitialState()
    {
        // This test verifies that StateChangesWithCurrent immediately considers current state

        // Arrange - Set pantry sensor to on before starting automation
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");

        // Create new automation to test initialization
        using var testAutomation = new LightAutomation(_entities, _mockLogger.Object);
        _mockHaContext.ClearServiceCalls();

        // Act - Start automation (should immediately check current state)
        testAutomation.StartAutomation();
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Trigger a slight state change to activate StateChangesWithCurrent logic
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Assert - Should turn on bathroom automation due to motion sensor state
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);
    }

    [Fact]
    public void ComplexUserScenario_PantryToBathroomFlow()
    {
        // This test simulates a realistic user flow: pantry → (brief overlap) → bathroom → both clear

        // Act - User enters pantry
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Verify pantry motion turns on bathroom automation
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);
        _mockHaContext.ClearServiceCalls();

        // User moves to bathroom (brief overlap period)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.BathroomMotionSensor)
        );

        // Pantry clears but bathroom still active - automation should stay on
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Verify automation doesn't turn off yet (bathroom still active)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
        _mockHaContext.ClearServiceCalls();

        // Finally, bathroom clears too - now both are off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.BathroomMotionSensor)
        );

        // Assert - Should NOT turn off immediately (30 seconds delay)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Now bathroom automation should turn off after delay
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);

        // Verify that the specific bathroom automation turn_off occurred (service call count may include sensor delay adjustments)
    }

    /// <summary>
    /// Tests timer cancellation when pantry motion is detected again before the 60-second delay expires.
    /// This verifies that Rx properly cancels pending timers when state changes.
    /// </summary>
    [Fact]
    public void PantryClearedThen55SecondsThenOccupiedAgain_Should_CancelTurnOffTimer()
    {
        // Arrange - Both sensors start as off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Pantry clears (starts 60s timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance 55 seconds (before timer expires)
        _mockHaContext.AdvanceTimeBySeconds(55);

        // Pantry motion detected again (should cancel timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Clear the TurnOn call from motion detection
        _mockHaContext.ClearServiceCalls();

        // Advance another 10 seconds (65 seconds total, but only 10 since re-trigger)
        _mockHaContext.AdvanceTimeBySeconds(10);

        // Assert - Timer was cancelled, should NOT have turned off
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    /// <summary>
    /// Tests that bathroom automation stays ON when bathroom becomes occupied during the pantry clear timer.
    /// This verifies the .Where() filter prevents turn-off when bathroom sensor is occupied.
    /// </summary>
    [Fact]
    public void BothClearedThen30SecondsThenBathroomOccupied_Should_PreventTurnOff()
    {
        // Arrange - Both sensors start as off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Pantry clears (starts 60s timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance 30 seconds (halfway through timer)
        _mockHaContext.AdvanceTimeBySeconds(30);

        // Bathroom becomes occupied (update state before timer fires)
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "on");
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.BathroomMotionSensor)
        );

        _mockHaContext.ClearServiceCalls();

        // Advance another 30 seconds (timer fires at 60s total)
        _mockHaContext.AdvanceTimeBySeconds(30);

        // Assert - .Where() filter should prevent turn-off because bathroom is occupied
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    /// <summary>
    /// Tests timing boundary - ensures timer doesn't fire before the full 60 seconds have elapsed.
    /// </summary>
    [Fact]
    public void BothClearedFor59Seconds_Should_NotTurnOffBathroomAutomation()
    {
        // Arrange - Both sensors start as off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Pantry clears (starts 60s timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance exactly 59 seconds (one second before timer should fire)
        _mockHaContext.AdvanceTimeBySeconds(59);

        // Assert - Timer should NOT have fired yet
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );
    }

    /// <summary>
    /// Tests resilience to rapid sensor flickering - multiple on/off cycles in quick succession.
    /// Verifies that multiple TurnOn() calls are handled gracefully without errors.
    /// </summary>
    [Fact]
    public void RapidPantryFlickering_Should_HandleMultipleTurnOnCalls()
    {
        // Arrange - Start with sensors off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate rapid flickering: off -> on -> off -> on -> off -> on (5 occupancy triggers)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Assert - Should have called TurnOn 3 times (redundant but harmless)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_on",
            3
        );

        // Verify no exceptions occurred during rapid state changes
        _mockHaContext.ServiceCalls.Should().NotBeEmpty();
    }

    /// <summary>
    /// Tests graceful degradation when bathroom sensor is stuck in occupied state (hardware failure).
    /// Verifies that .Where() filter prevents turn-off until bathroom sensor clears.
    /// </summary>
    [Fact]
    public void PantryClearedFor60Seconds_BathroomStuckOccupied_Should_KeepAutomationOn()
    {
        // Arrange - Pantry off, bathroom stuck at "on"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "on"); // Stuck sensor
        _mockHaContext.ClearServiceCalls();

        // Act - Pantry clears (starts 60s timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance full 60 seconds (timer fires)
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should NOT turn off because bathroom sensor is still "on"
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        // Verify automation remains resilient to stuck sensor (waits indefinitely)
    }

    /// <summary>
    /// Tests that redundant turn-off calls are handled gracefully when automation is already off.
    /// </summary>
    [Fact]
    public void PantryClearedWithAutomationAlreadyOff_Should_HandleGracefully()
    {
        // Arrange - Both sensors off, automation already off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionAutomation.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Pantry clears (starts 60s timer)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance 60 seconds (timer fires)
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should call TurnOff even though already off (redundant but harmless)
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);

        // This is expected behavior - the automation doesn't check current state before calling TurnOff
    }

    /// <summary>
    /// Tests multiple clear/re-occupy cycles to verify timer resets properly each time.
    /// Ensures that each new "clear" event cancels previous timers and starts a new 60s countdown.
    /// </summary>
    [Fact]
    public void MultipleClearCycles_Should_ResetTimerEachTime()
    {
        // Arrange - Both sensors start as off
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Cycle 1: Pantry clears (timer 1 starts)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance 40 seconds
        _mockHaContext.AdvanceTimeBySeconds(40);

        // Pantry occupied again, then clears (timer 2 starts, timer 1 cancelled)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        _mockHaContext.ClearServiceCalls();

        // Advance 40 seconds (80s total, but only 40s since last clear)
        _mockHaContext.AdvanceTimeBySeconds(40);

        // Assert - Should NOT turn off yet (only 40s since second clear)
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.BathroomMotionAutomation.EntityId,
            "turn_off",
            0
        );

        // Advance another 20 seconds (100s total, 60s since last clear)
        _mockHaContext.AdvanceTimeBySeconds(20);

        // Assert - NOW should turn off (60s since second clear)
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);
    }

    /// <summary>
    /// Tests that OnCleared() requires an actual state transition (occupied -> clear).
    /// Verifies that sensors already in "clear" state at initialization don't trigger the timer
    /// until there's an explicit occupied -> clear transition.
    /// </summary>
    [Fact]
    public void SensorsAlreadyClear_Should_OnlyStartTimerAfterOccupiedThenClearTransition()
    {
        // Arrange - Sensors start in "clear" state (simulating sensors that were off before automation started)
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BathroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Trigger occupied state first (this is required for OnCleared to fire later)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Verify TurnOn was called
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.BathroomMotionAutomation.EntityId);

        // Clear calls
        _mockHaContext.ClearServiceCalls();

        // Now trigger the clear transition (occupied -> clear)
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Advance 60 seconds
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should turn off because we had a complete occupied -> clear transition
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.BathroomMotionAutomation.EntityId);

        // This test verifies that OnCleared() observables require state transitions,
        // not just static "clear" states at initialization
    }

    #endregion

    public void Dispose()
    {
        SchedulerProvider.Reset();
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IPantryMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs
    /// </summary>
    private class TestEntities(IHaContext haContext) : IPantryLightEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.pantry_motion_sensor");
        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.pantry_motion_sensors");
        public LightEntity Light => new(haContext, "light.pantry_lights");
        public NumberEntity SensorDelay => new(haContext, "number.z_esp32_c6_3_still_target_delay");
        public BinarySensorEntity MiScalePresenceSensor =>
            new(haContext, "binary_sensor.esp32_presence_bedroom_mi_scale_presence");
        public LightEntity MirrorLight => new(haContext, "light.controller_rgb_df1c0d");
        public BinarySensorEntity BedroomDoor =>
            new(haContext, "binary_sensor.contact_sensor_door");
        public ButtonEntity Restart => new(haContext, "button.restart");

        public SwitchEntity BathroomMotionAutomation =>
            new(haContext, "switch.bathroom_motion_sensor");

        public BinarySensorEntity BathroomMotionSensor =>
            new(haContext, "binary_sensor.bathroom_motion_sensor");
    }
}
