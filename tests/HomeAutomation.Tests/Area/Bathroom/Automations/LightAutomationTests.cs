using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Bathroom.Automations;

/// <summary>
/// Comprehensive behavioral tests for Bathroom MotionAutomation using clean assertion syntax
/// Tests only automation behavior with mocked dimming controller for proper separation of concerns
/// </summary>
public class LightAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<ILogger<LightAutomation>> _mockLogger;
    private readonly Mock<IDimmingLightController> _mockDimmingController;
    private readonly TestEntities _entities;
    private readonly LightAutomation _automation;

    public LightAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LightAutomation>>();
        _mockScheduler = new Mock<IScheduler>();
        _mockDimmingController = new Mock<IDimmingLightController>();

        // Create test entities wrapper - much simpler!
        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(
            _entities,
            _mockDimmingController.Object,
            _mockScheduler.Object,
            _mockLogger.Object
        );

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void MotionDetected_Should_CallDimmingControllerOnMotionDetected()
    {
        // Act - Simulate motion sensor turning on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call dimming controller with light entity
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Once,
            "Should call OnMotionDetected on dimming controller when motion is detected"
        );
    }

    [Fact]
    public void MotionCleared_Should_CallDimmingControllerOnMotionStopped()
    {
        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call dimming controller async method
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should call OnMotionStoppedAsync on dimming controller when motion is cleared"
        );
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

        // Assert - Verify dimming controller calls in sequence
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Exactly(2),
            "Should call OnMotionDetected twice for two motion detected events"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should call OnMotionStoppedAsync once for motion cleared event"
        );
    }

    [Fact]
    public void NoMotion_Should_MakeNoDimmingControllerCalls()
    {
        // Act - Do nothing (no state changes)

        // Assert - Dimming controller should not be called
        _mockDimmingController.Verify(
            x => x.OnMotionDetected(It.IsAny<LightEntity>()),
            Times.Never,
            "Should not call OnMotionDetected when no motion events occur"
        );
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(It.IsAny<LightEntity>()),
            Times.Never,
            "Should not call OnMotionStoppedAsync when no motion events occur"
        );
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
    public void SensorDelay_Configuration_Should_PassThroughToDimmingController()
    {
        // This test verifies that the automation delegates sensor delay handling to the dimming controller
        // The actual sensor delay logic is tested in DimmingLightControllerTests

        // Act - Simulate motion cleared (sensor delay behavior is handled by dimming controller)
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
    public void MotionSensor_UnavailableToOff_Should_CallDimmingController_WhenNotIgnoringAvailability()
    {
        // Arrange - simulate motion sensor going on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Act - simulate motion sensor going on -> unavailable -> off
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "unavailable");
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "unavailable", "off");

        // Assert - The motion sensor off should still trigger OnMotionStoppedAsync
        _mockDimmingController.Verify(
            x => x.OnMotionStoppedAsync(_entities.Light),
            Times.Once,
            "Should still respond to unavailable → off transition when ignorePreviousUnavailable is false"
        );
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
        };

        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
        _mockDimmingController?.Object.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IMotionAutomationEntities interface
    /// Creates entities internally with the appropriate entity IDs for Bathroom
    /// </summary>
    private class TestEntities(IHaContext haContext) : ILightAutomationEntities
    {
        public SwitchEntity MasterSwitch { get; } =
            new SwitchEntity(haContext, "switch.bathroom_motion_sensor");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.bathroom_presence_sensors");
        public LightEntity Light { get; } = new LightEntity(haContext, "light.bathroom_lights");
        public NumberEntity SensorDelay { get; } =
            new NumberEntity(haContext, "number.z_esp32_c6_2_still_target_delay");
        public ButtonEntity Restart { get; } = new ButtonEntity(haContext, "button.restart");
    }
}
