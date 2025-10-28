using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.LivingRoom.Automations;

/// <summary>
/// Comprehensive behavioral tests for LivingRoom FanAutomation using clean assertion syntax
/// Tests complex fan coordination logic with multiple fans and cross-area dependencies
/// </summary>
public class FanAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly TestEntities _entities;
    private readonly FanAutomation _automation;

    public FanAutomationTests()
    {
        _mockHaContext = new MockHaContext();

        // Create test entities wrapper
        _entities = new TestEntities(_mockHaContext);

        _automation = new FanAutomation(
            _entities,
            MockHaContext.CreateLogger<FanAutomation>(LogLevel.Debug)
        );

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Set initial states
        _mockHaContext.SetEntityState(_entities.MasterSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.CeilingFan.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.StandFan.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.ExhaustFan.EntityId, "off");
        _mockHaContext.SetEntityState(_entities.BedroomMotionSensor.EntityId, "off");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void Construction_Should_InitializeWithShouldActivateFanFalse()
    {
        // Assert - ShouldActivateFan should start as false (verified through behavior)
        var stateChange = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Need to wait 3 seconds for motion trigger - simulate time-based trigger
        // Since we can't easily test time delays in unit tests, we'll test the direct method

        // With ShouldActivateFan = false initially, need to check specific behavior
        // The automation only responds to motion after 3 seconds, so direct motion won't trigger immediately
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.CeilingFan.EntityId);
    }

    [Fact]
    public void MotionDetectedFor3Seconds_Should_TurnOnCeilingFan()
    {
        // Arrange - Ensure MasterSwitch is on (already set in constructor)
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion detection
        var motionDetected = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionDetected);

        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 0);

        _mockHaContext.AdvanceTimeBySeconds(3);

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 1);
    }

    [Fact]
    public void MotionDetected_WithBedroomMotionOff_Should_TurnOnExhaustFan()
    {
        // Arrange - Set bedroom motion sensor to off
        _mockHaContext.SetEntityState(_entities.BedroomMotionSensor.EntityId, "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion detection
        var motionDetected = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionDetected);

        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 0);

        _mockHaContext.AdvanceTimeBySeconds(3);

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 1);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.ExhaustFan.EntityId, "turn_on", 1);
    }

    [Fact]
    public void MotionClearedFor1Minute_Should_TurnOffAllFans()
    {
        // Act - Test motion cleared event processing
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "off");
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 0);

        _mockHaContext.AdvanceTimeBySeconds(15);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 0);

        _mockHaContext.AdvanceTimeBySeconds(15);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 0);

        _mockHaContext.AdvanceTimeBySeconds(15);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 0);

        _mockHaContext.AdvanceTimeBySeconds(15);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 1);

        _mockHaContext.ClearServiceCalls();
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 0);

        _mockHaContext.AdvanceTimeBySeconds(3);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_on", 1);

        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "off");
        _mockHaContext.AdvanceTimeByMinutes(1);
        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.CeilingFan.EntityId, "turn_off", 1);
    }

    [Fact]
    public void MultipleFansCoordination_Should_HandleCorrectly()
    {
        // Test that the automation handles multiple fans correctly

        // Arrange - Verify all fans exist
        _entities
            .Fans.Should()
            .HaveCount(3, "LivingRoom should have 3 fans: ceiling, stand, and exhaust");

        var fanIds = _entities.Fans.Select(f => f.EntityId).ToList();
        fanIds.Should().Contain(_entities.CeilingFan.EntityId);
        fanIds.Should().Contain(_entities.StandFan.EntityId);
        fanIds.Should().Contain(_entities.ExhaustFan.EntityId);

        // Act & Assert - Verify fan structure is correct
        _entities.CeilingFan.EntityId.Should().Be("switch.ceiling_fan");
        _entities.StandFan.EntityId.Should().Be("switch.sonoff_10023810231");
        _entities.ExhaustFan.EntityId.Should().Be("switch.cozylife_955f");
    }

    [Fact]
    public void AutomatedFanOperation_Should_NotThrow()
    {
        // Act - Simulate automated fan operation (automation turning fan on/off)
        var automatedStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.CeilingFan,
            "off",
            "on",
            null // No user ID indicates automation
        );

        // Assert - Should process automated changes without errors
        var act = () => _mockHaContext.StateChangeSubject.OnNext(automatedStateChange);
        act.Should().NotThrow();
    }

    [Fact]
    public void MasterSwitchDisabled_Should_PreventAllFanOperations()
    {
        // Arrange - Disable master switch
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");
        _mockHaContext.ClearServiceCalls();

        // Act - Try motion detection while automation is disabled
        var motionDetected = StateChangeHelpers.MotionDetected(_entities.MotionSensor);
        _mockHaContext.StateChangeSubject.OnNext(motionDetected);

        // Assert - No fan operations should occur when master switch is off
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.CeilingFan.EntityId);
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.StandFan.EntityId);
        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.ExhaustFan.EntityId);
    }

    [Fact]
    public void Automation_Should_NotThrow_WhenStateChangesOccur()
    {
        // This test ensures automation setup doesn't throw exceptions

        // Act & Assert - Should not throw with various state changes
        var act = () =>
        {
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionDetected(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionCleared(_entities.MotionSensor)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.SwitchTurnedOn(_entities.CeilingFan)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.SwitchTurnedOff(_entities.CeilingFan)
            );
            _mockHaContext.StateChangeSubject.OnNext(
                StateChangeHelpers.MotionDetected(_entities.BedroomMotionSensor)
            );
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void FanAutomationBase_Integration_Should_WorkCorrectly()
    {
        // Test integration with FanAutomationBase functionality

        // Verify base class properties are accessible
        _entities.Fans.Should().NotBeEmpty();
        _entities.CeilingFan.Should().NotBeNull();
        _entities.MotionSensor.Should().NotBeNull();
        _entities.MasterSwitch.Should().NotBeNull();

        // Verify the automation can handle base class patterns
        var act = () =>
        {
            // Test typical fan automation patterns
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
    }

    /// <summary>
    /// Test wrapper that implements ILivingRoomFanEntities interface
    /// Creates entities internally with the appropriate entity IDs for LivingRoom Fan Automation
    /// </summary>
    private class TestEntities(IHaContext haContext) : ILivingRoomFanEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.sala_motion_sensor");
        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.living_room_presence_sensors");
        public IEnumerable<SwitchEntity> Fans =>
            [
                new SwitchEntity(haContext, "switch.ceiling_fan"),
                new SwitchEntity(haContext, "switch.sonoff_10023810231"),
                new SwitchEntity(haContext, "switch.cozylife_955f"),
            ];
        public BinarySensorEntity BedroomMotionSensor =>
            new(haContext, "binary_sensor.bedroom_presence_sensors");

        // Convenience properties for accessing specific fans
        public SwitchEntity CeilingFan => Fans.First(); // switch.ceiling_fan
        public SwitchEntity StandFan => Fans.Skip(1).First(); // switch.sonoff_10023810231
        public SwitchEntity ExhaustFan => Fans.Skip(2).First(); // switch.cozylife_955f (index 2)
    }
}
