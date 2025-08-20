using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Desk.Automations;

/// <summary>
/// Comprehensive behavioral tests for Desk MotionAutomation using clean assertion syntax
/// Tests desk-specific motion automation with presence detection and light/display control
/// Tests only automation behavior with mocked dimming controller for proper separation of concerns
/// </summary>
public class LightAutomationTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<LightAutomation>> _mockLogger;
    private readonly Mock<ILgDisplay> _mockLgDisplay;
    private readonly TestEntities _entities;
    private readonly LightAutomation _automation;

    public LightAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LightAutomation>>();
        _mockLgDisplay = new Mock<ILgDisplay>();

        // Create test entities wrapper for desk-specific entities
        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(_entities, _mockLgDisplay.Object, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
    }

    [Fact]
    public void MotionSensor_OnToUnavailable_ShouldBeIgnored()
    {
        // Arrange - motion ON
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");

        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - simulate motion sensor becoming unavailable
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "unavailable");

        // Assert
        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    [Fact]
    public void MotionSensor_OnToOff_ShouldTurnOffDisplay()
    {
        // Arrange - simulate motion detected
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");
        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId);
        _mockHaContext.ClearServiceCalls();

        // Act - simulate motion stops
        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "on", "off");

        // Assert
        _mockHaContext.ShouldHaveCalledLightTurnOff(_entities.Light.EntityId);
    }

    public void Dispose()
    {
        _automation?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test wrapper that implements IDeskMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs for Desk area
    /// Uses desk-specific entity IDs based on Home Assistant configuration
    /// </summary>
    private class TestEntities(IHaContext haContext) : IDeskLightEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.motion_sensors");
        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.desk_smart_presence");
        public LightEntity Light => new(haContext, "light.lg_display");
        public NumberEntity SensorDelay =>
            new(haContext, "number.z_esp32_c6_1_still_target_delay_2");

        public LightEntity SalaLights => new(haContext, "light.sala_lights");

        public ButtonEntity Restart => new(haContext, "button.restart");
    }
}
