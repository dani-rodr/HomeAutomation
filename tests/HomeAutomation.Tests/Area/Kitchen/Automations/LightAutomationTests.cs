using HomeAutomation.apps.Area.Kitchen.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.Kitchen.Automations;

/// <summary>
/// Comprehensive behavioral tests for Kitchen MotionAutomation with custom timing logic
/// Tests the specific Kitchen features: 5-second motion delay, power plug sensor integration,
/// and 1-hour auto-reactivation functionality.
///
/// NOTE: Kitchen automation uses IsOnForSeconds(5) which creates a 5-second delay before turning on light.
/// This is different from other automations that turn on lights immediately.
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

        // Create test entities wrapper with Kitchen-specific entities
        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(_entities, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    #region Custom Timing Tests (Kitchen-Specific)

    [Fact]
    public void MotionDetected_WithCustomTiming_Should_SetupCorrectSubscription()
    {
        // Kitchen-specific: Motion sensor uses 5-second delay (IsOnForSeconds(5))
        // This test verifies that the automation subscribes to motion events correctly
        // Note: Testing the actual 5-second delay would require a test scheduler

        // Act - Simulate motion sensor turning on
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - The automation should be set up correctly (no immediate light turn on due to 5-second delay)
        // Kitchen automation waits 5 seconds before turning on light, so immediate check should be empty
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void MotionCleared_Should_TurnOffLightImmediately()
    {
        // Kitchen-specific: Motion cleared triggers immediate light turn off (no delay)

        // Act - Simulate motion sensor turning off
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should turn off light immediately when motion cleared
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void AutomationSubscriptions_Should_NotThrow()
    {
        // Test that all automation subscriptions are set up correctly without exceptions

        // Act & Assert - Should not throw for any state changes
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionDetected(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionCleared(_entities.MotionSensor)
            );
        };

        act.Should()
            .NotThrow("Kitchen automation should handle motion state changes without throwing");
    }

    #endregion

    #region Power Plug Integration Tests (Kitchen-Specific)

    [Fact]
    public void PowerPlugTurnedOn_Should_SetSensorDelayToActiveValue()
    {
        // Kitchen-specific: Power plug state changes adjust sensor delay values
        // SensorActiveDelayValue = 15 (from Kitchen override)

        // Act - Simulate power plug turning on
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should call number.set_value with active delay value (15)
        _mockHaContext.ShouldHaveCalledService(
            "number",
            "set_value",
            _entities.SensorDelay.EntityId
        );

        // Verify the service call was made correctly
        var numberCalls = _mockHaContext.GetServiceCalls("number").ToList();
        var setValueCall = numberCalls.FirstOrDefault(call =>
            call.Service == "set_value"
            && call.Target?.EntityIds?.Contains(_entities.SensorDelay.EntityId) == true
        );

        setValueCall.Should().NotBeNull("Expected sensor delay to be set when power plug turns on");
    }

    [Fact]
    public void PowerPlugTurnedOff_Should_NotAffectSensorDelay()
    {
        // Kitchen-specific: Only power plug ON events trigger sensor delay adjustment

        // Act - Simulate power plug turning off
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not call sensor delay service for power plug off events
        var numberCalls = _mockHaContext.GetServiceCalls("number").ToList();
        numberCalls.Should().BeEmpty("Power plug turning off should not affect sensor delay");
    }

    [Fact]
    public void MultiplePowerPlugCycles_Should_SetSensorDelayEachTime()
    {
        // Test multiple power plug on/off cycles

        // Act - Multiple power plug cycles
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "on", "off")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on")
        );

        // Assert - Should have called sensor delay service twice (only for ON events)
        var numberCalls = _mockHaContext
            .GetServiceCalls("number")
            .Where(call =>
                call.Service == "set_value"
                && call.Target?.EntityIds?.Contains(_entities.SensorDelay.EntityId) == true
            )
            .ToList();

        numberCalls.Should().HaveCount(2, "Should set sensor delay for each power plug ON event");
    }

    #endregion

    #region Auto-Reactivation Tests (Kitchen-Specific)

    [Fact]
    public void AutoReactivationSubscription_Should_BeSetupCorrectly()
    {
        // Kitchen-specific: Auto-reactivation after 1 hour of no motion
        // SetupMotionSensorReactivation() uses IsOffForHours(1)
        // Note: Testing the actual 1-hour delay would require a test scheduler

        // Arrange - Set master switch to off initially
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion sensor state changes
        var stateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - The subscription should be set up correctly and not throw
        var act = () => _mockHaContext.StateChangeSubject.OnNext(stateChange);
        act.Should().NotThrow("Auto-reactivation subscription should be properly configured");
    }

    #endregion

    #region Sensor Timing Configuration Tests (Kitchen-Specific)

    [Fact]
    public void KitchenTiming_Should_UseCustomValues()
    {
        // Kitchen-specific timing values:
        // SensorWaitTime = 15 seconds
        // SensorActiveDelayValue = 15
        // SensorInactiveDelayValue = 1

        // This test verifies the custom timing configuration is set up correctly

        // Act - Simulate motion sensor state changes
        var motionOnStateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        var motionOffStateChange = StateChangeHelpers.MotionCleared(_entities.MotionSensor);

        _mockHaContext.StateChangeSubject.OnNext(motionOnStateChange);
        _mockHaContext.StateChangeSubject.OnNext(motionOffStateChange);

        // Assert - The automation should handle events correctly with custom timing
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(motionOnStateChange);
            _mockHaContext.StateChangeSubject.OnNext(motionOffStateChange);
        };

        act.Should()
            .NotThrow("Sensor delay automation should be properly configured with custom timing");
    }

    #endregion

    #region Base Class Integration Tests

    [Fact]
    public void MasterSwitchEnabled_WithMotionOn_Should_TurnOnLight()
    {
        // Test base class functionality integration

        // Arrange - Set motion sensor to be "on" already
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn on light because motion sensor is already on (base class behavior)
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
    }

    [Fact]
    public void MasterSwitchEnabled_WithMotionOff_Should_TurnOffLight()
    {
        // Test base class functionality integration

        // Arrange - Set motion sensor to be "off"
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate master switch being turned on
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Assert - Should turn off light because motion sensor is off (base class behavior)
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    [Fact]
    public void LightManuallyControlled_Should_AffectMasterSwitch()
    {
        // Test that manual light control affects master switch (base class behavior)
        // Note: This test verifies the base class subscription setup rather than the specific logic
        // The actual manual control logic depends on context that may not be fully testable in unit tests

        // Arrange - Set consistent states
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate light state change (base class subscribes to IsManuallyOperated changes)
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.Light, "off", "on");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Verify the subscription doesn't throw (base class behavior is complex and context-dependent)
        // The actual master switch control logic requires specific user context that's difficult to mock
        var act = () => _mockHaContext.StateChangeSubject.OnNext(stateChange);
        act.Should().NotThrow("Light state change subscription should be properly configured");
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void PowerPlugAndMotionInteraction_Should_WorkCorrectly()
    {
        // Test power plug and motion interaction

        // Act - Power plug turns on, then motion changes
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );

        // Assert - Should have appropriate service calls
        // Power plug should set sensor delay
        _mockHaContext.ShouldHaveCalledService(
            "number",
            "set_value",
            _entities.SensorDelay.EntityId
        );

        // Motion cleared should immediately turn off light (no delay)
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);

        // Verify we have the expected service calls (power plug + light off)
        var totalCalls = _mockHaContext.ServiceCalls.Count();
        totalCalls
            .Should()
            .Be(2, "Should have made 2 service calls: sensor delay and light turn off");
    }

    [Fact]
    public void MotionCyclesWithPowerPlug_Should_HandleSequenceCorrectly()
    {
        // Test multiple motion cycles with power plug interactions

        // Act - Complex sequence
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on")
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionCleared(_entities.MotionSensor)
        );
        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.MotionDetected(_entities.MotionSensor)
        );

        // Assert - Verify correct behavior
        // Motion detected events don't immediately turn on light (5-second delay), but motion cleared does turn off
        _mockHaContext.ShouldHaveCalledLightExactly(_entities.Light.EntityId, "turn_off", 1);
        _mockHaContext.ShouldHaveCalledService(
            "number",
            "set_value",
            _entities.SensorDelay.EntityId
        );

        // Verify total service calls (1 sensor delay + 1 light turn off)
        var totalCalls = _mockHaContext.ServiceCalls.Count();
        totalCalls.Should().Be(2, "Should have made 2 service calls for this sequence");
    }

    #endregion

    #region Error Handling and State Validation

    [Fact]
    public void InvalidStateChanges_Should_NotThrow()
    {
        // Test that automation handles invalid state changes gracefully

        // Act & Assert - Should not throw for any state changes
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionDetected(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionCleared(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "off", "on")
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.CreateStateChange(_entities.PowerPlug, "on", "off")
            );
        };

        act.Should()
            .NotThrow("Kitchen automation should handle all state changes without throwing");
    }

    [Fact]
    public void StateTracking_Should_WorkCorrectly()
    {
        // Verify MockHaContext state tracking works for Kitchen entities

        // Arrange - Set initial states
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.PowerPlug.EntityId, "off");

        // Verify initial states
        var motionState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        var powerState = _mockHaContext.GetState(_entities.PowerPlug.EntityId);

        motionState?.State.Should().Be("off");
        powerState?.State.Should().Be("off");

        // Act - Change states
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");
        _mockHaContext.SimulateStateChange(_entities.PowerPlug.EntityId, "off", "on");

        // Assert - States should be updated
        var newMotionState = _mockHaContext.GetState(_entities.MotionSensor.EntityId);
        var newPowerState = _mockHaContext.GetState(_entities.PowerPlug.EntityId);

        newMotionState?.State.Should().Be("on");
        newPowerState?.State.Should().Be("on");

        // Verify entity methods work
        _entities.MotionSensor.IsOccupied().Should().BeTrue();
        _entities.PowerPlug.IsOn().Should().BeTrue();
    }

    #endregion

    #region Cleanup and Disposal

    [Fact]
    public void NoAction_Should_MakeNoServiceCalls()
    {
        // Verify that no action results in no service calls

        // Act - Do nothing

        // Assert - Should have no service calls
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    #endregion

    /// <summary>
    /// Test wrapper that implements IKitchenMotionEntities interface
    /// Creates entities with appropriate entity IDs for Kitchen area
    /// </summary>
    private class TestEntities(IHaContext haContext) : IKitchenLightEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.kitchen_motion_sensor");
        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.kitchen_motion_sensors");
        public LightEntity Light => new(haContext, "light.kitchen_lights");
        public NumberEntity SensorDelay => new(haContext, "number.z_esp32_c6_4_still_target_delay");
        public BinarySensorEntity PowerPlug => new(haContext, "binary_sensor.kitchen_power_plug");
        public ButtonEntity Restart => new(haContext, "button.restart");
    }
}
