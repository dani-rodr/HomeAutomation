using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Common.Interface;
using HomeAutomation.apps.Common.Services.Factories;

namespace HomeAutomation.Tests.Common.Base;

public class MotionSensorBaseTests
{
    private readonly Mock<ITypedEntityFactory> _factory = new();
    private readonly Mock<IMotionSensorRestartScheduler> _scheduler = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly MockHaContext _mockHaContext = new();
    private readonly BinarySensorEntity _smartPresence;
    private readonly BinarySensorEntity _presence;
    private readonly ButtonEntity _clear;
    private readonly ButtonEntity _restart;
    private readonly SwitchEntity _masterSwitch;
    private readonly SwitchEntity _engineeringMode;
    private TestMotionSensorBase _sut;

    public MotionSensorBaseTests()
    {
        _smartPresence = new(_mockHaContext, "binary_sensor.device_smart_presence");
        _presence = new(_mockHaContext, "binary_sensor.device_presence");
        _clear = new(_mockHaContext, "button.device_manual_clear");
        _restart = new(_mockHaContext, "button.device_restart_esp32");
        _masterSwitch = new(_mockHaContext, "switch.device_auto_calibrate");
        _engineeringMode = new(_mockHaContext, "switch.device_engineering_mode");

        _factory
            .Setup(f => f.Create<BinarySensorEntity>("device", "smart_presence"))
            .Returns(_smartPresence);
        _factory.Setup(f => f.Create<BinarySensorEntity>("device", "presence")).Returns(_presence);
        _factory.Setup(f => f.Create<ButtonEntity>("device", "manual_clear")).Returns(_clear);
        _factory.Setup(f => f.Create<ButtonEntity>("device", "restart_esp32")).Returns(_restart);
        _factory
            .Setup(f => f.Create<SwitchEntity>("device", "auto_calibrate"))
            .Returns(_masterSwitch);
        _factory
            .Setup(f => f.Create<SwitchEntity>("device", "engineering_mode"))
            .Returns(_engineeringMode);

        _sut = new TestMotionSensorBase(
            _factory.Object,
            _scheduler.Object,
            "device",
            _logger.Object
        );
    }

    [Fact]
    public void ShouldTriggerClearWhenRecoveredFromUnavailableAndPresenceIsClear()
    {
        // Arrange - Set up initial states
        _mockHaContext.SetEntityState(_smartPresence.EntityId, "unavailable");
        _mockHaContext.SetEntityState(_presence.EntityId, "off"); // Clear state
        _mockHaContext.SetEntityState(_masterSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_engineeringMode.EntityId, "off");

        // Mock scheduler to return empty (no daily restart schedule for this test)
        _scheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);

        // Act - Start the automation to set up subscriptions
        _sut.StartAutomation();

        // Simulate SmartPresence recovering from unavailable to clear (off)
        _mockHaContext.SimulateStateChange(_smartPresence.EntityId, "unavailable", "off");

        // Assert - Clear button should be pressed when SmartPresence recovers and Presence is clear
        _mockHaContext.ShouldHaveCalledButtonPress(_clear.EntityId);
    }

    internal class TestMotionSensorBase(
        ITypedEntityFactory factory,
        IMotionSensorRestartScheduler scheduler,
        string deviceName,
        ILogger logger
    ) : MotionSensorBase(factory, scheduler, deviceName, logger) { }
}
