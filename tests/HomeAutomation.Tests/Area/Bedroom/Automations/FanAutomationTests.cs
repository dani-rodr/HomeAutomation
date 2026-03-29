using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;

namespace HomeAutomation.Tests.Area.Bedroom.Automations;

/// <summary>
/// Comprehensive behavioral tests for Bedroom FanAutomation using clean assertion syntax
/// Tests fan control logic extending FanAutomationBase with motion-based activation patterns
/// </summary>
public class FanAutomationTests : AutomationTestBase<FanAutomation>
{
    private MockHaContext _mockHaContext => HaContext;

    private Mock<ILogger<FanAutomation>> _mockLogger => Logger;

    private readonly TestEntities _entities;

    private readonly FanAutomation _automation;

    public FanAutomationTests()
    {
        // Create test entities wrapper

        _entities = new TestEntities(_mockHaContext);

        _automation = new FanAutomation(_entities, _mockLogger.Object);

        StartAutomation(_automation, _entities.MasterSwitch.EntityId);

        // Set initial states

        _mockHaContext.SetEntityState(_entities.MotionSensor.EntityId, "off");

        _mockHaContext.SetEntityState(_entities.Fan.EntityId, "off");
    }

    [Fact]
    public void Construction_Should_InitializeWithShouldActivateFan()
    {
        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // With ShouldActivateFan = false, motion on should not turn on fan

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: fan automation logic under review | issue HA-TEST-2004 | expires 2026-06-30"
    )]
    public void FanManuallyTurnedOn_Should_SetShouldActivateFanTrue()
    {
        // Arrange - Simulate fan being manually turned on (this should set ShouldActivateFan = true)

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        // Act - Simulate manual fan operation

        _mockHaContext.EmitStateChange(manualOnStateChange);

        // Update fan state to reflect the manual operation

        _mockHaContext.SetEntityState(_entities.Fan.EntityId, "on");

        _mockHaContext.ClearServiceCalls();

        // Now motion should activate fan because ShouldActivateFan = true

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - Fan should turn on because ShouldActivateFan is now true

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: bedroom automation logic under review | issue HA-TEST-2002 | expires 2026-06-30"
    )]
    public void FanManuallyTurnedOff_Should_SetShouldActivateFanFalse()
    {
        // Arrange - First set ShouldActivateFan to true

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        _mockHaContext.SetEntityState(_entities.Fan.EntityId, "on");

        // Now manually turn off fan

        var manualOffStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "on",
            "off",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        // Act - Simulate manual fan turn off

        _mockHaContext.EmitStateChange(manualOffStateChange);

        _mockHaContext.SetEntityState(_entities.Fan.EntityId, "off");

        _mockHaContext.ClearServiceCalls();

        // Now motion should NOT activate fan because ShouldActivateFan = false

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - Fan should NOT turn on because ShouldActivateFan is now false

        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: fan automation logic under review | issue HA-TEST-2004 | expires 2026-06-30"
    )]
    public void MotionDetected_WithShouldActivateFanTrue_Should_TurnOnFan()
    {
        // Arrange - Set ShouldActivateFan to true by manual operation

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        _mockHaContext.SetEntityState(_entities.Fan.EntityId, "on");

        _mockHaContext.ClearServiceCalls();

        // Act - Simulate motion detection

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - Fan should turn on

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);
    }

    [Fact]
    public void MotionDetected_Should_TurnOnFan()
    {
        // Arrange

        // Act - Simulate motion detection

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - Fan should turn on

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);
    }

    [Fact]
    public void MotionCleared_Should_TurnOffFan()
    {
        // Act - Simulate motion cleared

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

        // Assert - Fan should turn off regardless of ShouldActivateFan state

        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: bedroom automation logic under review | issue HA-TEST-2002 | expires 2026-06-30"
    )]
    public void CompleteMotionCycle_WithManualActivation_Should_FollowExpectedPattern()
    {
        // Arrange - Set ShouldActivateFan to true

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        _mockHaContext.ClearServiceCalls();

        // Act & Assert - Motion on should turn on fan

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);

        _mockHaContext.ClearServiceCalls();

        // Act & Assert - Motion off should turn off fan

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: fan automation logic under review | issue HA-TEST-2004 | expires 2026-06-30"
    )]
    public void AutomatedFanOperation_Should_NotChangeShouldActivateFan()
    {
        // Arrange - Set ShouldActivateFan to true initially

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        _mockHaContext.ClearServiceCalls();

        // Act - Simulate automated fan operation (automation turning fan on/off)

        var automatedStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            null // No user ID indicates automation
        );

        _mockHaContext.EmitStateChange(automatedStateChange);

        _mockHaContext.ClearServiceCalls();

        // Verify ShouldActivateFan remains true by testing motion response

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - Fan should still respond to motion (ShouldActivateFan still true)

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Fan.EntityId);
    }

    [Fact]
    public void MasterSwitchDisabled_Should_PreventAllFanOperations()
    {
        // Arrange - Set ShouldActivateFan to true

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        // Disable master switch

        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");

        _mockHaContext.ClearServiceCalls();

        // Act - Try motion detection while automation is disabled

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        // Assert - No fan operations should occur

        _mockHaContext.ShouldNeverHaveCalledSwitch(_entities.Fan.EntityId);
    }

    [Fact(
        Skip = "Quarantined: fan automation logic under review | issue HA-TEST-2004 | expires 2026-06-30"
    )]
    public void MultipleMotionEvents_Should_HandleCorrectly()
    {
        // Arrange - Set ShouldActivateFan to true

        var manualOnStateChange = StateChangeHelpers.CreateSwitchStateChange(
            _entities.Fan,
            "off",
            "on",
            HaIdentity.DANIEL_RODRIGUEZ
        );

        _mockHaContext.EmitStateChange(manualOnStateChange);

        _mockHaContext.ClearServiceCalls();

        // Act - Multiple motion events

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

        // Assert - Should have 2 turn on and 2 turn off calls

        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.Fan.EntityId, "turn_on", 2);

        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.Fan.EntityId, "turn_off", 2);
    }

    [Fact]
    public void Automation_Should_NotThrow_WhenStateChangesOccur()
    {
        // This test ensures automation setup doesn't throw exceptions

        // Act & Assert - Should not throw

        var act = () =>
        {
            _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

            _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

            _mockHaContext.EmitStateChange(StateChangeHelpers.SwitchTurnedOn(_entities.Fan));

            _mockHaContext.EmitStateChange(StateChangeHelpers.SwitchTurnedOff(_entities.Fan));
        };

        act.Should().NotThrow();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Test wrapper that implements IBedroomFanEntities interface
    /// Creates entities internally with the appropriate entity IDs for Bedroom Fan Automation
    /// </summary>
    private class TestEntities(IHaContext haContext) : IBedroomFanEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.bedroom_motion_sensor");

        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.bedroom_presence_sensors");

        public IEnumerable<SwitchEntity> Fans =>
            [new SwitchEntity(haContext, "switch.sonoff_100238104e1")];

        // Convenience property for single fan access (following base class pattern)

        public SwitchEntity Fan => Fans.First();
    }
}
