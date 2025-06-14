using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.Pantry.Automations;

/// <summary>
/// Comprehensive behavioral tests for Pantry MotionAutomation using clean assertion syntax
/// Verifies actual automation behavior with enhanced readability
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

        // Create test entities wrapper - much simpler!
        _entities = new TestEntities(_mockHaContext);

        _automation = new MotionAutomation(_entities, _mockLogger.Object);

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
        _mockHaContext.ShouldHaveCalledBothLightsTurnOff(_entities.Light.EntityId, _entities.MirrorLight.EntityId);
    }

    [Fact]
    public void MiScalePresenceDetected_Should_TurnOnMirrorLight()
    {
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
        var motionStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionStateChange);

        var presenceStateChange = StateChangeHelpers.PresenceDetected(_entities.MiScalePresenceSensor);
        _mockHaContext.StateChangeSubject.OnNext(presenceStateChange);

        // Assert - Verify both lights and exact counts using entity IDs
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.MirrorLight.EntityId);
        _mockHaContext.ShouldHaveServiceCallCount(2);
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
        // Act - Motion on, off, on again
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionCleared(_entities.MotionSensor));
        _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.MotionDetected(_entities.MotionSensor));

        // Assert - Verify exact call counts for complex scenarios using entity IDs
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.MirrorLight.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveServiceCallCount(4); // on, off, off (mirror), on
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
        _entities.MotionSensor.IsOccupied().Should().BeTrue("motion sensor should report occupied after state change");
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
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.PresenceDetected(_entities.MiScalePresenceSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.DoorClosed(_entities.BedroomDoor));
        };

        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IPantryMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs
    /// </summary>
    private class TestEntities(IHaContext haContext) : IPantryMotionEntities
    {
        public SwitchEntity MasterSwitch { get; } = new SwitchEntity(haContext, "switch.pantry_motion_sensor");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.pantry_motion_sensors");
        public LightEntity Light { get; } = new LightEntity(haContext, "light.pantry_lights");
        public NumberEntity SensorDelay { get; } =
            new NumberEntity(haContext, "number.z_esp32_c6_3_still_target_delay");
        public BinarySensorEntity MiScalePresenceSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.esp32_presence_bedroom_mi_scale_presence");
        public LightEntity MirrorLight { get; } = new LightEntity(haContext, "light.controller_rgb_df1c0d");
        public BinarySensorEntity BedroomDoor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.contact_sensor_door");
    }
}
