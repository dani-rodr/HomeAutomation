using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.LivingRoom.Automations;

/// <summary>
/// Comprehensive behavioral tests for LivingRoom TabletAutomation using clean assertion syntax
/// Tests tablet screen control logic based on motion detection with simplified sensor delay behavior
/// </summary>
public class TabletAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<IScheduler> _mockScheduler;
    private readonly Mock<ILogger<TabletAutomation>> _mockLogger;
    private readonly TestTabletEntities _entities;
    private readonly TabletAutomation _automation;

    public TabletAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockScheduler = new Mock<IScheduler>();
        _mockLogger = new Mock<ILogger<TabletAutomation>>();

        // Create test entities wrapper
        _entities = new TestTabletEntities(_mockHaContext);

        _automation = new TabletAutomation(_entities, _mockScheduler.Object, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void MotionDetected_Should_TurnOnTabletScreen()
    {
        // Act - Simulate motion sensor turning on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn on tablet screen (treated as light entity)
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MotionCleared_Should_TurnOffTabletScreen()
    {
        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn off tablet screen
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOn_Should_TurnOnTabletScreen()
    {
        // Arrange - Set motion sensor to be "on" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on (should trigger ControlLightOnMotionChange)
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn on tablet screen because motion sensor is already on
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOff_Should_TurnOffTabletScreen()
    {
        // Arrange - Set motion sensor to be "off"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn off tablet screen because motion sensor is off
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void TabletScreenControl_Should_FollowMotionSensorChanges()
    {
        // This test verifies the core functionality: tablet screen follows motion sensor state

        // Test case 1: Motion ON -> Screen ON
        // Act - Simulate motion detection
        var motionOnStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionOnStateChange);

        // Assert - Should turn on tablet screen
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Test case 2: Motion OFF -> Screen OFF
        // Act - Simulate motion cleared
        var motionOffStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionOffStateChange);

        // Assert - Should turn off tablet screen
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

        // Assert - Verify exact call counts for tablet screen
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveServiceCallCount(3); // on, off, on
    }

    [Fact]
    public void SensorDelayAutomations_Should_ReturnEmpty()
    {
        // This verifies the overridden behavior that returns empty sensor delay automations
        // Act - Access the automation's sensor delay configuration indirectly via motion timing

        // Motion detection should happen immediately without sensor delay logic
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should immediately turn on tablet screen (no delay automation)
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);

        // No sensor delay entity is involved since TabletAutomation returns empty from GetSensorDelayAutomations
        // This test verifies the simplified behavior compared to other motion automations
    }

    [Fact]
    public void NoMotion_Should_MakeNoServiceCalls()
    {
        // Act - Do nothing (no state changes)

        // Assert - Clean negative assertion
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void ComplexScenario_With_MotionSequenceHandling()
    {
        // Act - Complex motion sequence: on -> off -> on -> off
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

        // Assert - Verify exact pattern for tablet screen control
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_on", 2);
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 2);
        _mockHaContext.ShouldHaveServiceCallCount(4); // on, off, on, off
    }

    [Fact]
    public void StateTracking_Should_Work_Correctly()
    {
        // This test verifies our MockHaContext state tracking works for tablet entities

        // Arrange - Set initial state
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");

        // Verify initial states
        var initialMotionState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        var initialScreenState = _mockHaContext.GetState(_entities.Light.EntityId);
        initialMotionState?.State.Should().Be("off");
        initialScreenState?.State.Should().Be("off");

        // Act - Simulate state changes
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");

        // Assert - States should be updated correctly
        var newMotionState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        newMotionState?.State.Should().Be("on");

        // Verify entity IsOccupied() works correctly
        _entities
            .MotionSensor.IsOccupied()
            .Should()
            .BeTrue("motion sensor should report occupied after state change");
    }

    [Fact]
    public void TabletActiveEntity_Should_BeAccessible()
    {
        // This test verifies the TabletActive entity is properly configured
        // Even though it's not used in current automation logic, it's part of the interface

        // Arrange & Act - Access the tablet active entity
        var tabletActiveEntity = _entities.TabletActive;

        // Assert - Entity should be properly configured
        tabletActiveEntity.Should().NotBeNull("TabletActive entity should be available");
        tabletActiveEntity
            .EntityId.Should()
            .Be("binary_sensor.mipad", "TabletActive should have correct entity ID");
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
                StateChangeHelpers.CreateStateChange(_entities.Light, "off", "on")
            );
            _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void MasterSwitchDisabled_Should_NotRespondToMotion()
    {
        // Arrange - Turn off master switch to disable automation
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion detection while automation is disabled
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not turn on tablet screen when automation is disabled
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void AutomationReEnabled_Should_SyncTabletScreenWithMotionState()
    {
        // Arrange - Disable automation and set motion to on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Re-enable automation (master switch on)
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should sync tablet screen with current motion state
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements ITabletEntities interface
    /// Creates entities internally with the appropriate entity IDs for LivingRoom tablet
    /// </summary>
    private class TestTabletEntities(IHaContext haContext) : ITabletEntities
    {
        public SwitchEntity MasterSwitch { get; } =
            new SwitchEntity(haContext, "switch.sala_motion_sensor");
        public BinarySensorEntity MotionSensor { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.living_room_presence_sensors");
        public LightEntity Light { get; } = new LightEntity(haContext, "light.mipad_screen");
        public BinarySensorEntity TabletActive { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.mipad");

        public NumberEntity SensorDelay { get; } =
            new NumberEntity(haContext, "Number.Ld2410Esp321StillTargetDelay");
        public ButtonEntity Restart { get; } = new ButtonEntity(haContext, "button.restart");
    }
}
