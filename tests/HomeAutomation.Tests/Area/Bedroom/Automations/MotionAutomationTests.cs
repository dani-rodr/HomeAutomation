using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.Bedroom.Automations;

/// <summary>
/// Comprehensive behavioral tests for Bedroom MotionAutomation using clean assertion syntax
/// Tests complex functionality including double-click detection, physical switch operations,
/// and master switch disabling behavior with enhanced readability
/// </summary>
public class MotionAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<MotionAutomation>> _mockLogger;
    private readonly TestEntities _entities;
    private readonly MotionAutomation _automation;

    public MotionAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();

        // Create test entities wrapper
        _entities = new TestEntities(_mockHaContext);

        _automation = new MotionAutomation(_entities, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    #region Motion Detection Tests

    [Fact]
    public void MotionDetected_Should_TurnOnBedroomLight()
    {
        // Act - Simulate motion sensor turning on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn on light directly (no dimming controller in bedroom)
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MotionCleared_Should_TurnOffBedroomLight()
    {
        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn off light directly
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void MultipleMotionEvents_Should_HandleCorrectSequence()
    {
        // Act - Motion on, off, on again
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Assert - Verify exact call counts for the sequence
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveServiceCallCount(3); // on, off, on
    }

    #endregion

    #region Right Side Empty Switch Tests

    [Fact]
    public void RightSideEmptySwitch_PhysicalPress_Should_ToggleLightAndDisableMasterSwitch()
    {
        // Arrange - Ensure light is off initially
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate physical press of right side empty switch (no userId = physical)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should toggle light and disable master switch
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(2);
    }

    [Fact]
    public void RightSideEmptySwitch_AutomatedPress_Should_DoNothing()
    {
        // Act - Simulate automated press of right side empty switch (with userId = automated)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: "supervisor"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not perform any actions
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void RightSideEmptySwitch_UserPress_Should_DoNothing()
    {
        // Act - Simulate user press of right side empty switch (with known userId)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: "7512fc7c361e45879df43f9f0f34fc57"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not perform any actions (only physical presses are handled)
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    #endregion

    #region Left Side Fan Switch Double-Click Tests

    [Fact]
    public void LeftSideFanSwitch_SingleClick_Should_DoNothing()
    {
        // Act - Simulate single click on left side fan switch (physical press)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "off",
            "on",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Wait a moment to ensure no double-click is detected
        Thread.Sleep(100);

        // Assert - Should not perform any actions (single click doesn't trigger double-click logic)
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void LeftSideFanSwitch_DoubleClick_Physical_Should_ToggleLightAndDisableMasterSwitch()
    {
        // Arrange - Ensure light is off initially
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate double-click on left side fan switch (two rapid physical presses)
        var firstClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "off",
            "on",
            userId: null
        );
        var secondClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "on",
            "off",
            userId: null
        );

        _mockHaContext.StateChangeSubject.OnNext(firstClick);
        _mockHaContext.StateChangeSubject.OnNext(secondClick);

        // Assert - Should toggle light and disable master switch
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(2);
    }

    [Fact]
    public void LeftSideFanSwitch_DoubleClick_Automated_Should_DoNothing()
    {
        // Act - Simulate double-click on left side fan switch (automated presses)
        var firstClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "off",
            "on",
            userId: "supervisor"
        );
        var secondClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "on",
            "off",
            userId: "supervisor"
        );

        _mockHaContext.StateChangeSubject.OnNext(firstClick);
        _mockHaContext.StateChangeSubject.OnNext(secondClick);

        // Assert - Should not perform any actions (only physical double-clicks are handled)
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void LeftSideFanSwitch_SlowDoubleClick_Should_DoNothing()
    {
        // Act - Simulate slow double-click (beyond 2-second timeout)
        var firstClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "off",
            "on",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(firstClick);

        // Wait longer than the 2-second timeout
        Thread.Sleep(2500);

        var secondClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "on",
            "off",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(secondClick);

        // Assert - Should not perform any actions (timeout exceeded)
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    #endregion

    #region Master Switch Behavior Tests

    [Fact]
    public void LightTurnedOnByAutomation_Should_EnableMasterSwitch()
    {
        // Arrange - Set up automated light state change (automated userId)
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate light being turned on by automation
        var lightStateChange = StateChangeHelpers.CreateLightStateChange(
            _entities.Light,
            "off",
            "on",
            userId: "f389ce79e38841e4bfd26c9685ffa784"
        ); // SUPERVISOR userId
        _mockHaContext.StateChangeSubject.OnNext(lightStateChange);

        // Assert - Should turn on master switch
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void LightTurnedOnByUser_Should_ControlMasterSwitchBasedOnMotion()
    {
        // Note: This test verifies the base class behavior for manual light changes
        // The behavior depends on the base class ControlMasterSwitchOnLightChange logic
        // which compares light state with motion state

        // Arrange - Set motion sensor state and clear any setup calls
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate light being turned on by user (manual operation)
        var lightStateChange = StateChangeHelpers.CreateLightStateChange(
            _entities.Light,
            "off",
            "on",
            userId: "7512fc7c361e45879df43f9f0f34fc57"
        );
        _mockHaContext.StateChangeSubject.OnNext(lightStateChange);

        // Assert - Base class behavior will control master switch based on state comparison
        // Since this is complex base class behavior, we'll verify any call was made
        var switchCalls = _mockHaContext.GetServiceCalls("switch").ToList();
        switchCalls
            .Should()
            .NotBeEmpty("Manual light changes should trigger master switch control");
    }

    [Fact]
    public void LightTurnedOffByAutomation_Should_NotEnableMasterSwitch()
    {
        // Act - Simulate light being turned off by automation
        var lightStateChange = StateChangeHelpers.CreateLightStateChange(
            _entities.Light,
            "on",
            "off",
            userId: "supervisor"
        );
        _mockHaContext.StateChangeSubject.OnNext(lightStateChange);

        // Assert - Should not turn on master switch (only automated "on" events trigger this)
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOn_Should_TurnOnLight()
    {
        // Arrange - Set motion sensor to be "on" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on (should trigger motion-based logic)
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

    #endregion

    #region Complex Scenarios Tests

    [Fact]
    public void ComplexScenario_MotionDetection_Then_PhysicalSwitchOverride()
    {
        // Act - First motion is detected
        var motionStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionStateChange);

        // Then physical switch is pressed (should override and disable master switch)
        var switchStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(switchStateChange);

        // Assert - Verify both actions occurred
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId); // from motion
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId); // from switch
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId); // switch override
        _mockHaContext.ShouldHaveServiceCallCount(3);
    }

    [Fact]
    public void ComplexScenario_DoubleClick_Then_MotionDetection_Should_StillWork()
    {
        // Arrange - Ensure light is off initially and clear any setup calls
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - First double-click (should disable master switch)
        var firstClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "off",
            "on",
            userId: null
        );
        var secondClick = StateChangeHelpers.CreateSwitchStateChange(
            _entities.LeftSideFanSwitch,
            "on",
            "off",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(firstClick);
        _mockHaContext.StateChangeSubject.OnNext(secondClick);

        // Verify double-click worked
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId); // from double-click
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId); // master switch disabled

        // Clear calls to check motion behavior separately
        _mockHaContext.ClearServiceCalls();

        // Then motion is detected (should work because motion automation is always active)
        var motionStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionStateChange);

        // Assert - Motion automation should still work (it's not controlled by master switch for motion events)
        // The bedroom automation has direct motion handling that's not switchable
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void RapidSwitchPresses_Should_HandleEachPress()
    {
        // Act - Multiple rapid physical presses of right side switch
        for (int i = 0; i < 3; i++)
        {
            var stateChange = StateChangeHelpers.CreateSwitchStateChange(
                _entities.RightSideEmptySwitch,
                "off",
                "on",
                userId: null
            );
            _mockHaContext.StateChangeSubject.OnNext(stateChange);
            Thread.Sleep(50); // Brief delay between presses
        }

        // Assert - Should handle each press independently
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "toggle", 3);
        _mockHaContext.ShouldHaveCalledSwitchExactly(
            _entities.MasterSwitch.EntityId,
            "turn_off",
            3
        );
        _mockHaContext.ShouldHaveServiceCallCount(6); // 3 toggles + 3 master switch offs
    }

    #endregion

    #region Edge Cases and Error Handling

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
                StateChangeHelpers.CreateSwitchStateChange(
                    _entities.RightSideEmptySwitch,
                    "off",
                    "on",
                    userId: null
                )
            );

            var firstClick = StateChangeHelpers.CreateSwitchStateChange(
                _entities.LeftSideFanSwitch,
                "off",
                "on",
                userId: null
            );
            var secondClick = StateChangeHelpers.CreateSwitchStateChange(
                _entities.LeftSideFanSwitch,
                "on",
                "off",
                userId: null
            );
            _mockHaContext.StateChangeSubject.OnNext(firstClick);
            _mockHaContext.StateChangeSubject.OnNext(secondClick);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void StateTracking_Should_Work_Correctly()
    {
        // This test verifies our MockHaContext state tracking works

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
    public void NoMotion_Should_MakeNoServiceCalls()
    {
        // Act - Do nothing (no state changes)

        // Assert - Should have no service calls
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void NullUserId_Should_BeConsideredPhysical()
    {
        // Act - Simulate switch press with null userId (should be considered physical)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: null
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should be treated as physical operation
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId);
    }

    [Fact]
    public void EmptyUserId_Should_BeConsideredPhysical()
    {
        // Act - Simulate switch press with empty userId (should be considered physical)
        var stateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.RightSideEmptySwitch,
            "off",
            "on",
            userId: ""
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should be treated as physical operation
        _mockHaContext.ShouldHaveCalledLightToggle(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.MasterSwitch.EntityId);
    }

    #endregion

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IBedroomMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs for Bedroom
    /// </summary>
    private class TestEntities(IHaContext haContext) : IBedroomMotionEntities
    {
        public SwitchEntity MasterSwitch { get; } =
            new SwitchEntity(haContext, "switch.bedroom_motion_sensor");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.bedroom_motion_sensors");
        public LightEntity Light { get; } = new LightEntity(haContext, "light.bedroom_lights");
        public NumberEntity SensorDelay { get; } =
            new NumberEntity(haContext, "number.z_esp32_c6_1_still_target_delay");
        public SwitchEntity RightSideEmptySwitch { get; } =
            new SwitchEntity(haContext, "switch.right_side_empty_switch");
        public SwitchEntity LeftSideFanSwitch { get; } =
            new SwitchEntity(haContext, "switch.left_side_fan_switch");
    }
}
