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

    // Test entities as private fields - easy to edit right here!
    private readonly SwitchEntity _masterSwitch;
    private readonly BinarySensorEntity _motionSensor;
    private readonly LightEntity _pantryLight;
    private readonly LightEntity _mirrorLight;
    private readonly BinarySensorEntity _miScalePresence;
    private readonly BinarySensorEntity _roomDoor;
    private readonly NumberEntity _sensorDelay;

    private readonly TestEntities _entities;
    private readonly MotionAutomation _automation;

    public MotionAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();

        // Initialize test entities with descriptive entity IDs
        _masterSwitch = new SwitchEntity(_mockHaContext, "switch.pantry_motion_sensor");
        _motionSensor = new BinarySensorEntity(_mockHaContext, "binary_sensor.pantry_motion_sensors");
        _pantryLight = new LightEntity(_mockHaContext, "light.pantry_lights");
        _sensorDelay = new NumberEntity(_mockHaContext, "number.z_esp32_c6_3_still_target_delay");
        _miScalePresence = new BinarySensorEntity(
            _mockHaContext,
            "binary_sensor.esp32_presence_bedroom_mi_scale_presence"
        );
        _mirrorLight = new LightEntity(_mockHaContext, "light.controller_rgb_df1c0d");
        _roomDoor = new BinarySensorEntity(_mockHaContext, "binary_sensor.contact_sensor_door");

        // Create wrapper that implements the interface
        _entities = new TestEntities(
            _masterSwitch,
            _motionSensor,
            _pantryLight,
            _sensorDelay,
            _miScalePresence,
            _mirrorLight,
            _roomDoor
        );

        _automation = new MotionAutomation(_entities, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "off", "on");

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
        _mockHaContext.ShouldHaveCalledLightTurnOn(_pantryLight.EntityId);
    }

    [Fact]
    public void MotionCleared_Should_TurnOffBothLights()
    {
        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Clean helper for common pattern using entity IDs
        _mockHaContext.ShouldHaveCalledBothLightsTurnOff(_pantryLight.EntityId, _mirrorLight.EntityId);
    }

    [Fact]
    public void MiScalePresenceDetected_Should_TurnOnMirrorLight()
    {
        // Act - Simulate MiScale presence sensor turning on
        var stateChange = StateChangeHelpers.PresenceDetected(_entities.MiScalePresenceSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Clean syntax with negative assertions using entity IDs
        _mockHaContext.ShouldHaveCalledLightTurnOn(_mirrorLight.EntityId);
        _mockHaContext.ShouldNeverHaveCalledLight(_pantryLight.EntityId);
    }

    [Fact]
    public void RoomDoorClosed_Should_TurnOnMasterSwitch()
    {
        // Act - Simulate room door closing (IsOff means closed)
        var stateChange = StateChangeHelpers.DoorClosed(_entities.RoomDoor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Switch assertions work too using entity ID
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_masterSwitch.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOn_Should_TurnOnLight()
    {
        // Arrange - Set motion sensor to be "on" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on (should trigger ControlLightOnMotionChange)
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "off", "on");

        // Assert - Should turn on light because motion sensor is already on
        _mockHaContext.ShouldHaveCalledLightTurnOn(_pantryLight.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOff_Should_TurnOffLight()
    {
        // Arrange - Set motion sensor to be "off"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "off", "on");

        // Assert - Should turn off light because motion sensor is off
        _mockHaContext.ShouldHaveCalledLightTurnOff(_pantryLight.EntityId);
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
        _mockHaContext.ShouldHaveCalledLightTurnOn(_pantryLight.EntityId);
        _mockHaContext.ShouldHaveCalledLightTurnOn(_mirrorLight.EntityId);
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
        _mockHaContext.ShouldHaveCalledLightExactly(_pantryLight.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_pantryLight.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveCalledLightExactly(_mirrorLight.EntityId, "turn_off", 1);
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
            _mockHaContext.StateChangeSubject.OnNext(StateChangeHelpers.DoorClosed(_entities.RoomDoor));
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
    /// </summary>
    private class TestEntities(
        SwitchEntity masterSwitch,
        BinarySensorEntity motionSensor,
        LightEntity light,
        NumberEntity sensorDelay,
        BinarySensorEntity miScalePresenceSensor,
        LightEntity mirrorLight,
        BinarySensorEntity roomDoor
    ) : IPantryMotionEntities
    {
        public SwitchEntity MasterSwitch { get; } = masterSwitch;
        public BinarySensorEntity MotionSensor { get; } = motionSensor;
        public LightEntity Light { get; } = light;
        public NumberEntity SensorDelay { get; } = sensorDelay;
        public BinarySensorEntity MiScalePresenceSensor { get; } = miScalePresenceSensor;
        public LightEntity MirrorLight { get; } = mirrorLight;
        public BinarySensorEntity RoomDoor { get; } = roomDoor;
    }
}
