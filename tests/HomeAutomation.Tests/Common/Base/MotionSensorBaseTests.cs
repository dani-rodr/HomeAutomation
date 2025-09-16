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
    private readonly NumberEntity _sensorDelay;
    private readonly List<NumberEntity> _moveThresholds = [];
    private readonly List<NumberEntity> _stillThresholds = [];
    private readonly List<NumericSensorEntity> _moveEnergies = [];
    private readonly List<NumericSensorEntity> _stillEnergies = [];
    private TestMotionSensorBase _sut;

    public MotionSensorBaseTests()
    {
        _smartPresence = new(_mockHaContext, "binary_sensor.device_smart_presence");
        _presence = new(_mockHaContext, "binary_sensor.device_presence");
        _clear = new(_mockHaContext, "button.device_manual_clear");
        _restart = new(_mockHaContext, "button.device_restart_esp32");
        _masterSwitch = new(_mockHaContext, "switch.device_auto_calibrate");
        _engineeringMode = new(_mockHaContext, "switch.device_engineering_mode");
        _sensorDelay = new(_mockHaContext, "number.device_still_target_delay");

        // Initialize zone entities (9 zones)
        for (int i = 0; i < 9; i++)
        {
            _moveThresholds.Add(
                new NumberEntity(_mockHaContext, $"number.device_g{i}_move_threshold")
            );
            _stillThresholds.Add(
                new NumberEntity(_mockHaContext, $"number.device_g{i}_still_threshold")
            );
            _moveEnergies.Add(
                new NumericSensorEntity(_mockHaContext, $"sensor.device_g{i}_move_energy")
            );
            _stillEnergies.Add(
                new NumericSensorEntity(_mockHaContext, $"sensor.device_g{i}_still_energy")
            );
        }

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
        _factory
            .Setup(f => f.Create<NumberEntity>("device", "still_target_delay"))
            .Returns(_sensorDelay);

        // Setup zone entity factories
        for (int i = 0; i < 9; i++)
        {
            var index = i; // Capture for closure
            _factory
                .Setup(f => f.Create<NumberEntity>("device", $"g{index}_move_threshold"))
                .Returns(_moveThresholds[index]);
            _factory
                .Setup(f => f.Create<NumberEntity>("device", $"g{index}_still_threshold"))
                .Returns(_stillThresholds[index]);
            _factory
                .Setup(f => f.Create<NumericSensorEntity>("device", $"g{index}_move_energy"))
                .Returns(_moveEnergies[index]);
            _factory
                .Setup(f => f.Create<NumericSensorEntity>("device", $"g{index}_still_energy"))
                .Returns(_stillEnergies[index]);
        }

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

    [Fact]
    public void LogMotionTrigger_Should_NotBeCalled_When_AutoCalibrateTransitionsFromUnavailableToOff()
    {
        // Arrange - Set up zone entities with test data
        SetupZoneTestData();
        
        // Set initial states - MasterSwitch unavailable, SmartPresence occupied
        _mockHaContext.SetEntityState(_masterSwitch.EntityId, "unavailable");
        _mockHaContext.SetEntityState(_smartPresence.EntityId, "on"); // Occupied
        _mockHaContext.SetEntityState(_engineeringMode.EntityId, "off");

        // Mock scheduler to return empty (no daily restart schedule for this test)
        _scheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);

        // Act - Start the automation to set up subscriptions
        _sut.StartAutomation();
        _logger.Reset(); // Clear any initialization logging

        // Simulate auto_calibrate (MasterSwitch) transitioning from unavailable to off
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "unavailable", "off");

        // Assert - LogMotionTrigger should NOT be called because MasterSwitch is off
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MoveEnergy") && v.ToString()!.Contains("MoveThreshold")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "LogMotionTrigger should not be called when MasterSwitch transitions to off");
    }

    [Fact]
    public void LogMotionTrigger_Should_BeCalled_When_SmartPresenceOccupied_And_MasterSwitchOn()
    {
        // Arrange - Set up zone entities with test data
        SetupZoneTestData();
        
        // Set initial states - MasterSwitch on, SmartPresence clear
        _mockHaContext.SetEntityState(_masterSwitch.EntityId, "on");
        _mockHaContext.SetEntityState(_smartPresence.EntityId, "off");
        _mockHaContext.SetEntityState(_engineeringMode.EntityId, "on");

        // Mock scheduler to return empty
        _scheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);

        // Act - Start the automation and enable toggleable automations
        _sut.StartAutomation();
        
        // Simulate master switch change to enable automation (from off to on to trigger automation enable)
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "off", "on");
        _logger.Reset(); // Clear initialization logging

        // Simulate SmartPresence becoming occupied
        _mockHaContext.SimulateStateChange(_smartPresence.EntityId, "off", "on");

        // Assert - LogMotionTrigger should be called because MasterSwitch is on
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("g0_move_energy") && v.ToString()!.Contains("MoveThreshold")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce,
            "LogMotionTrigger should be called when SmartPresence becomes occupied and MasterSwitch is on");
    }

    [Fact]
    public void LogMotionTrigger_Should_NotBeCalled_When_SmartPresenceOccupied_But_MasterSwitchOff()
    {
        // Arrange - Set up zone entities with test data
        SetupZoneTestData();
        
        // Set initial states - MasterSwitch off, SmartPresence clear
        _mockHaContext.SetEntityState(_masterSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_smartPresence.EntityId, "off");
        _mockHaContext.SetEntityState(_engineeringMode.EntityId, "off");

        // Mock scheduler to return empty
        _scheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);

        // Act - Start the automation (toggleable automations should not be enabled)
        _sut.StartAutomation();
        _logger.Reset(); // Clear initialization logging

        // Simulate SmartPresence becoming occupied while MasterSwitch is off
        _mockHaContext.SimulateStateChange(_smartPresence.EntityId, "off", "on");

        // Assert - LogMotionTrigger should NOT be called because MasterSwitch is off
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("MoveEnergy") && v.ToString()!.Contains("MoveThreshold")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never,
            "LogMotionTrigger should not be called when MasterSwitch is off");
    }

    [Fact]
    public void EngineeringMode_Should_FollowAutoCalibrateStateChanges()
    {
        // Arrange - Set initial states
        _mockHaContext.SetEntityState(_masterSwitch.EntityId, "off");
        _mockHaContext.SetEntityState(_engineeringMode.EntityId, "off");

        // Mock scheduler to return empty
        _scheduler.Setup(s => s.GetSchedules(It.IsAny<Action>())).Returns([]);

        // Act & Assert - Start the automation
        _sut.StartAutomation();
        _mockHaContext.ClearServiceCalls();

        // Test: auto_calibrate turns on -> engineering mode should turn on
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "off", "on");
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_engineeringMode.EntityId);
        
        _mockHaContext.ClearServiceCalls();

        // Test: auto_calibrate turns off -> engineering mode should turn off
        _mockHaContext.SimulateStateChange(_masterSwitch.EntityId, "on", "off");
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_engineeringMode.EntityId);
    }

    private void SetupZoneTestData()
    {
        // Set up zone test data - make zone 0 trigger LogMotionTrigger
        for (int i = 0; i < 9; i++)
        {
            if (i == 0)
            {
                // Zone 0: MoveEnergy (50) > MoveThreshold (30) - should trigger logging
                _mockHaContext.SetEntityState(_moveEnergies[i].EntityId, "50");
                _mockHaContext.SetEntityState(_moveThresholds[i].EntityId, "30");
            }
            else
            {
                // Other zones: MoveEnergy (10) <= MoveThreshold (50) - should not trigger logging
                _mockHaContext.SetEntityState(_moveEnergies[i].EntityId, "10");
                _mockHaContext.SetEntityState(_moveThresholds[i].EntityId, "50");
            }
            // Set still thresholds and energies (not used in LogMotionTrigger)
            _mockHaContext.SetEntityState(_stillThresholds[i].EntityId, "20");
            _mockHaContext.SetEntityState(_stillEnergies[i].EntityId, "15");
        }
    }

    internal class TestMotionSensorBase(
        ITypedEntityFactory factory,
        IMotionSensorRestartScheduler scheduler,
        string deviceName,
        ILogger logger
    ) : MotionSensorBase(factory, scheduler, deviceName, logger) { }
}
