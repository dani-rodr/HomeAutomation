using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Desk.Automations;

/// <summary>
/// Comprehensive behavioral tests for Desk MotionAutomation using clean assertion syntax
/// Tests desk-specific motion automation with presence detection and light/display control
/// Tests only automation behavior with mocked dimming controller for proper separation of concerns
/// </summary>
public class MotionAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<MotionAutomation>> _mockLogger;
    private readonly Mock<IDimmingLightController> _mockDimmingController;
    private readonly TestEntities _entities;
    private readonly MotionAutomation _automation;

    public MotionAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();
        _mockDimmingController = new Mock<IDimmingLightController>();

        // Create test entities wrapper for desk-specific entities
        _entities = new TestEntities(_mockHaContext);

        _automation = new MotionAutomation(_entities, _mockDimmingController.Object, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void DeskPresenceDetected_Should_CallDimmingControllerOnMotionDetected()
    {
        // Act - Simulate desk presence sensor detecting presence
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call dimming controller with light entity
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Once,
            "Should call OnMotionDetected on dimming controller when desk presence is detected"
        );
    }

    [Fact]
    public void DeskPresenceCleared_Should_CallDimmingControllerOnMotionStopped()
    {
        // Act - Simulate desk presence sensor clearing presence
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call dimming controller async method
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should call OnMotionStoppedAsync on dimming controller when desk presence is cleared"
        );
    }

    [Fact]
    public void MasterSwitchEnabled_WithPresenceDetected_Should_TurnOnLight()
    {
        // Arrange - Set desk presence sensor to "detected" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on (should trigger ControlLightOnMotionChange)
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn on light because presence is already detected
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithNoPresence_Should_TurnOffLight()
    {
        // Arrange - Set desk presence sensor to "clear"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn off light because no presence is detected
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchDisabled_Should_PreventMotionProcessing()
    {
        // Arrange - Turn off master switch
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate presence detection while automation is disabled
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not call dimming controller when automation is disabled
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(It.IsAny<LightEntity>()),
            Times.Never,
            "Should not process motion events when master switch is disabled"
        );
    }

    [Fact]
    public void MultiplePresenceEvents_Should_HandleCorrectSequence()
    {
        // Act - Presence detected, cleared, detected again
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));

        // Assert - Verify dimming controller calls in sequence
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Exactly(2),
            "Should call OnMotionDetected twice for two presence detected events"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should call OnMotionStoppedAsync once for presence cleared event"
        );
    }

    [Fact]
    public void DeskSpecific_LongPresenceSession_Should_HandleExtendedDetection()
    {
        // Act - Simulate typical desk work session: presence detected and held
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));

        // Simulate additional presence events (sensor refresh/re-detection)
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));

        // Assert - Should handle multiple detection events gracefully
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Exactly(3),
            "Should handle multiple presence detection events during extended desk session"
        );

        // No motion stopped calls should occur
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Never,
            "Should not call OnMotionStoppedAsync when presence is continuously detected"
        );
    }

    [Fact]
    public void NoPresence_Should_MakeNoDimmingControllerCalls()
    {
        // Act - Do nothing (no state changes)

        // Assert - Dimming controller should not be called
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(It.IsAny<LightEntity>()),
            Times.Never,
            "Should not call OnMotionDetected when no presence events occur"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(It.IsAny<LightEntity>()),
            Times.Never,
            "Should not call OnMotionStoppedAsync when no presence events occur"
        );
    }

    [Fact]
    public void StateTracking_Should_Work_Correctly()
    {
        // This test verifies our MockHaContext state tracking works for desk presence

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

        // Verify entity IsOccupied() works correctly
        _entities
            .MotionSensor.IsOccupied()
            .Should()
            .BeTrue("desk presence sensor should report occupied after state change");
    }

    [Fact]
    public void SensorDelay_Configuration_Should_PassThroughToDimmingController()
    {
        // This test verifies that the automation delegates sensor delay handling to the dimming controller
        // The actual sensor delay logic is tested in DimmingLightControllerTests

        // Act - Simulate presence cleared (sensor delay behavior is handled by dimming controller)
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should delegate to dimming controller
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should delegate motion stopped handling to dimming controller which handles sensor delay logic"
        );
    }

    [Fact]
    public void DeskSpecific_PresenceIntermittent_Should_HandleWorkPatterns()
    {
        // Test typical desk work patterns where presence may be intermittent

        // Act - Simulate intermittent presence typical of desk work
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));

        // Brief pause, then back to work
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));

        // Assert - Should handle intermittent patterns correctly
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Exactly(2),
            "Should handle intermittent presence detection during desk work"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Exactly(2),
            "Should handle intermittent presence clearing during desk work"
        );
    }

    [Fact]
    public void Automation_Should_NotThrow_WhenStateChangesOccur()
    {
        // This test ensures automation setup doesn't throw exceptions

        // Act & Assert - Should not throw
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void DeskSpecific_RapidStateChanges_Should_HandleGracefully()
    {
        // Test rapid state changes that might occur with sensitive desk presence detection

        // Act - Simulate rapid state changes
        for (int i = 0; i < 5; i++)
        {
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));
        }

        // Assert - Should handle rapid changes without issue
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Exactly(5),
            "Should handle rapid presence detection events"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Exactly(5),
            "Should handle rapid presence clearing events"
        );
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
        _mockDimmingController?.Object.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IDeskMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs for Desk area
    /// Uses desk-specific entity IDs based on Home Assistant configuration
    /// </summary>
    private class TestEntities(IHaContext haContext) : IDeskMotionEntities
    {
        public SwitchEntity MasterSwitch { get; } = new SwitchEntity(haContext, "switch.motion_sensors");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.desk_smart_presence");
        public LightEntity Light { get; } = new LightEntity(haContext, "light.rgb_light_strip");
        public NumberEntity SensorDelay { get; } =
            new NumberEntity(haContext, "number.z_esp32_c6_1_still_target_delay_2");
    }
}
