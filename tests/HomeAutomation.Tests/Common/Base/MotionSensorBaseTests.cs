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
    private readonly Mock<BinarySensorEntity> _smartPresence;
    private readonly Mock<BinarySensorEntity> _presence;
    private readonly Mock<ButtonEntity> _clear;
    private readonly Mock<ButtonEntity> _restart;
    private readonly Mock<SwitchEntity> _masterSwitch;
    private readonly Mock<SwitchEntity> _engineeringMode;
    private TestMotionSensorBase _sut;

    public MotionSensorBaseTests()
    {
        _smartPresence = new(_mockHaContext, "smart_presence");
        _presence = new(_mockHaContext, "presence");
        _clear = new(_mockHaContext, "manual_clear");
        _restart = new(_mockHaContext, "restart_esp32");
        _masterSwitch = new(_mockHaContext, "auto_calibrate");
        _engineeringMode = new(_mockHaContext, "engineering_mode");

        _factory
            .Setup(f => f.Create<BinarySensorEntity>("device", "smart_presence"))
            .Returns(_smartPresence.Object);
        _factory
            .Setup(f => f.Create<BinarySensorEntity>("device", "presence"))
            .Returns(_presence.Object);
        _factory
            .Setup(f => f.Create<ButtonEntity>("device", "manual_clear"))
            .Returns(_clear.Object);
        _factory
            .Setup(f => f.Create<ButtonEntity>("device", "restart_esp32"))
            .Returns(_restart.Object);
        _factory
            .Setup(f => f.Create<SwitchEntity>("device", "auto_calibrate"))
            .Returns(_masterSwitch.Object);
        _factory
            .Setup(f => f.Create<SwitchEntity>("device", "engineering_mode"))
            .Returns(_masterSwitch.Object);

        _sut = new TestMotionSensorBase(
            _factory.Object,
            _scheduler.Object,
            "device",
            _logger.Object
        );
    }

    [Fact(Skip = "To be fixed")]
    public void ShouldTriggerClearWhenRecoveredFromUnavailableAndPresenceIsClear()
    {
        _smartPresence
            .Setup(e => e.StateChanges())
            .Returns(
                Observable.Empty<
                    StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
                >()
            );

        _presence
            .Setup(e => e.StateChanges())
            .Returns(
                Observable.Empty<
                    StateChange<BinarySensorEntity, EntityState<BinarySensorAttributes>>
                >()
            );

        _clear
            .Setup(e => e.StateChanges())
            .Returns(Observable.Empty<StateChange<ButtonEntity, EntityState<ButtonAttributes>>>());

        _restart
            .Setup(e => e.StateChanges())
            .Returns(Observable.Empty<StateChange<ButtonEntity, EntityState<ButtonAttributes>>>());

        _masterSwitch
            .Setup(e => e.StateChanges())
            .Returns(Observable.Empty<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>());

        _sut.StartAutomation();
    }

    internal class TestMotionSensorBase(
        ITypedEntityFactory factory,
        IMotionSensorRestartScheduler scheduler,
        string deviceName,
        ILogger logger
    ) : MotionSensorBase(factory, scheduler, deviceName, logger) { }
}
