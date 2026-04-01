using HomeAutomation.apps.Area.Bathroom.Automations;
using HomeAutomation.apps.Area.Bathroom.Automations.Entities;
using HomeAutomation.apps.Area.Bathroom.Config;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Bathroom.Automations;

/// <summary>
/// Comprehensive behavioral tests for Bathroom MotionAutomation using clean assertion syntax
/// Tests only automation behavior with mocked dimming controller for proper separation of concerns
/// </summary>
public class LightAutomationTests : AutomationTestBase<LightAutomation>
{
    private MockHaContext _mockHaContext => HaContext;

    private Mock<ILogger<LightAutomation>> _mockLogger => Logger;

    private readonly Mock<IDimmingLightController> _mockDimmingController;

    private readonly TestEntities _entities;

    private readonly LightAutomation _automation;

    public LightAutomationTests()
    {
        _mockDimmingController = new Mock<IDimmingLightController>();

        // Create test entities wrapper - much simpler!

        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(
            _entities,
            CreateSettings().Light,
            _mockDimmingController.Object,
            _mockLogger.Object
        );

        // Start the automation to set up subscriptions

        StartAutomation(_automation, _entities.MasterSwitch.EntityId);
    }

    private static BathroomSettings CreateSettings() =>
        new()
        {
            Light = new BathroomLightSettings
            {
                MotionOnDelaySeconds = 2,
                MasterSwitchDisableDelayMinutes = 5,
            },
        };

    [Fact]
    public void MotionDetected_Should_CallDimmingControllerOnMotionDetected()
    {
        // Act - Simulate motion sensor turning on

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

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

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

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

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

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

        _mockHaContext.EmitMotionCleared(_entities.MotionSensor);

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

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

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
            _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

            _mockHaContext.EmitMotionCleared(_entities.MotionSensor);
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void MasterSwitch_Should_NotEnable_WhenMotionDetected_For2Seconds_AndMasterSwitch_IsOffFor5Minutes()
    {
        // Arrange - Set master switch to be off for 5 minutes

        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");

        _mockHaContext.AdvanceTimeByMinutes(5);

        // Act - Simulate motion sensor turning on for 2 seconds

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.AdvanceTimeBySeconds(2);

        // Assert

        _mockDimmingController.Verify(
            x => x.OnMotionDetected(_entities.Light),
            Times.Never,
            "Light shouldn't turn on when it wasn't turn on by pantry motion sensor"
        );

        _mockHaContext.ShouldHaveCalledSwitchExactly(_entities.MasterSwitch.EntityId, "turn_on", 0);
    }

    [Fact]
    public void MasterSwitch_Should_Enable_WhenMotionDetected_For2Seconds_AndMasterSwitch_WasOn()
    {
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Arrange - Set master switch to be off for 4 minutes only

        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "on", "off");

        _mockHaContext.AdvanceTimeByMinutes(4);

        // Act - Simulate motion sensor turning on for 2 seconds

        _mockHaContext.EmitMotionDetected(_entities.MotionSensor);

        _mockHaContext.AdvanceTimeBySeconds(2);

        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.MasterSwitch.EntityId);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation.Dispose();
            _mockDimmingController.Object.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Test wrapper that implements IMotionAutomationEntities interface
    /// Creates entities internally with the appropriate entity IDs for Bathroom
    /// </summary>
    private class TestEntities(IHaContext haContext) : IBathroomLightEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.bathroom_motion_sensor");

        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.bathroom_presence_sensors");

        public LightEntity Light => new(haContext, "light.bathroom_lights");

        public NumberEntity SensorDelay => new(haContext, "number.z_esp32_c6_2_still_target_delay");

        public ButtonEntity Restart => new(haContext, "button.restart");
    }
}
