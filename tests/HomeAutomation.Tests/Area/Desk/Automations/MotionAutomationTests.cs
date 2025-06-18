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
    private readonly Mock<ILgDisplay> _mockLgDisplay;
    private readonly TestEntities _entities;
    private readonly MotionAutomation _automation;

    public MotionAutomationTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();
        _mockLgDisplay = new Mock<ILgDisplay>();

        // Create test entities wrapper for desk-specific entities
        _entities = new TestEntities(_mockHaContext);

        _automation = new MotionAutomation(_entities, _mockLgDisplay.Object, _mockLogger.Object);

        // Start the automation to set up subscriptions
        _automation.StartAutomation();

        // Simulate master switch being ON to enable automation logic
        _mockHaContext.SimulateStateChange(_entities.MasterSwitch.EntityId, "off", "on");

        // Clear any initialization service calls
        _mockHaContext.ClearServiceCalls();
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
