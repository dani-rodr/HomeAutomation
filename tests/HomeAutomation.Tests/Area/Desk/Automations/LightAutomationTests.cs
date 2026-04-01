using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Automations.Entities;
using HomeAutomation.apps.Area.Desk.Config;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Desk.Automations;

/// <summary>
/// Comprehensive behavioral tests for Desk MotionAutomation using clean assertion syntax
/// Tests desk-specific motion automation with presence detection and light/display control
/// Tests only automation behavior with mocked dimming controller for proper separation of concerns
/// </summary>
public class LightAutomationTests : AutomationTestBase<LightAutomation>
{
    private MockHaContext _mockHaContext => HaContext;

    private Mock<ILogger<LightAutomation>> _mockLogger => Logger;

    private readonly Mock<ILgDisplay> _mockLgDisplay;

    private readonly TestEntities _entities;

    private readonly LightAutomation _automation;

    private Subject<string> _sourceChangeSubject = new();

    public LightAutomationTests()
    {
        _mockLgDisplay = new Mock<ILgDisplay>();

        _mockLgDisplay.Setup(m => m.IsShowingPc).Returns(true);

        _mockLgDisplay.Setup(m => m.IsOff()).Returns(false);

        _mockLgDisplay.Setup(m => m.OnSourceChange()).Returns(_sourceChangeSubject.AsObservable());

        // Create test entities wrapper for desk-specific entities

        _entities = new TestEntities(_mockHaContext);

        _automation = new LightAutomation(
            _entities,
            CreateSettings().Light,
            _mockLgDisplay.Object,
            _mockLogger.Object
        );

        StartAutomation(_automation, _entities.MasterSwitch.EntityId);
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

    [Fact]
    public void SalaLights_On_ShouldTurnOnLightWithHighBrightness()
    {
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "on");

        // Act

        _mockHaContext.SimulateStateChange(_entities.SalaLights.EntityId, "off", "on");

        _mockHaContext.ShouldNeverHaveCalledLight(_entities.Light.EntityId);

        _mockHaContext.AdvanceTimeByMilliseconds(1);

        // Assert

        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId, 230);
    }

    [Fact]
    public void SalaLights_Off_ShouldTurnOnLightWithLowBrightness()
    {
        _mockHaContext.SetEntityState(_entities.Light.EntityId, "on");

        // Act

        _mockHaContext.SimulateStateChange(_entities.SalaLights.EntityId, "on", "off");

        _mockHaContext.ShouldNeverHaveCalledLight(_entities.Light.EntityId);

        _mockHaContext.AdvanceTimeByMilliseconds(1);

        // Assert

        _mockHaContext.ShouldHaveCalledLightTurnOn(_entities.Light.EntityId, 125);
    }

    [Fact]
    public void MotionSensor_ShouldNotToggleLight_WhenMonitorIsOff()
    {
        _mockLgDisplay.Setup(m => m.IsOff()).Returns(true);

        // Act

        _mockHaContext.SimulateStateChange(_entities.MotionSensor.EntityId, "off", "on");

        // Assert

        _mockHaContext.ShouldHaveNoServiceCalls();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Test wrapper that implements IDeskMotionEntities interface
    /// Creates entities internally with the appropriate entity IDs for Desk area
    /// Uses desk-specific entity IDs based on Home Assistant configuration
    /// </summary>
    private class TestEntities(IHaContext haContext) : IDeskLightEntities
    {
        public SwitchEntity MasterSwitch => new(haContext, "switch.LgTvMotionSensor");

        public BinarySensorEntity MotionSensor =>
            new(haContext, "binary_sensor.desk_smart_presence");

        public LightEntity Light => new(haContext, "light.lg_display");

        public NumberEntity SensorDelay =>
            new(haContext, "number.z_esp32_c6_1_still_target_delay_2");

        public LightEntity SalaLights => new(haContext, "light.sala_lights");

        public ButtonEntity Restart => new(haContext, "button.restart");
    }

    private static DeskSettings CreateSettings() => new() { Light = new DeskLightSettings() };
}
