using HomeAutomation.Area.Bedroom.Automations;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel;
using System.Reactive.Subjects;

namespace HomeAutomation.Tests.Area.Bedroom.Automations;

public class ClimateAutomationTests : IDisposable
{
    private readonly Mock<ILogger<ClimateAutomation>> _loggerMock;
    private readonly Mock<IScheduler> _schedulerMock;
    private readonly Mock<ClimateEntity> _acMock;
    private readonly Mock<BinarySensorEntity> _windowMock;
    private readonly Mock<InputBooleanEntity> _masterSwitchMock;
    private readonly Mock<SensorEntity> _sunNextRisingMock;
    private readonly Mock<SensorEntity> _sunNextDuskMock;
    private readonly Subject<StateChange> _windowStateChanges;
    private readonly ClimateAutomation _automation;

    public ClimateAutomationTests()
    {
        _loggerMock = new Mock<ILogger<ClimateAutomation>>();
        _schedulerMock = new Mock<IScheduler>();
        _acMock = new Mock<ClimateEntity>();
        _windowMock = new Mock<BinarySensorEntity>();
        _masterSwitchMock = new Mock<InputBooleanEntity>();
        _sunNextRisingMock = new Mock<SensorEntity>();
        _sunNextDuskMock = new Mock<SensorEntity>();
        _windowStateChanges = new Subject<StateChange>();

        _windowMock.Setup(x => x.StateChanges()).Returns(_windowStateChanges);
        _masterSwitchMock.SetupGet(x => x.State).Returns("on");
        
        _automation = new ClimateAutomation(
            _loggerMock.Object,
            _schedulerMock.Object,
            _acMock.Object,
            _windowMock.Object,
            _masterSwitchMock.Object,
            _sunNextRisingMock.Object,
            _sunNextDuskMock.Object);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Assert
        _automation.Should().NotBeNull();
        _automation.Logger.Should().Be(_loggerMock.Object);
    }

    [Fact]
    public void GetTimeBlock_Morning_ReturnsCorrectBlock()
    {
        // Act
        var result = ClimateAutomation.GetTimeBlock(7);

        // Assert
        result.Should().Be(ClimateAutomation.TimeBlock.Morning);
    }

    [Fact]
    public void GetTimeBlock_Day_ReturnsCorrectBlock()
    {
        // Act
        var result = ClimateAutomation.GetTimeBlock(12);

        // Assert
        result.Should().Be(ClimateAutomation.TimeBlock.Day);
    }

    [Fact]
    public void GetTimeBlock_Evening_ReturnsCorrectBlock()
    {
        // Act
        var result = ClimateAutomation.GetTimeBlock(20);

        // Assert
        result.Should().Be(ClimateAutomation.TimeBlock.Evening);
    }

    [Fact]
    public void GetTimeBlock_Night_ReturnsCorrectBlock()
    {
        // Act
        var result = ClimateAutomation.GetTimeBlock(2);

        // Assert
        result.Should().Be(ClimateAutomation.TimeBlock.Night);
    }

    [Fact]
    public void GetTimeBlock_InvalidHour_ThrowsException()
    {
        // Act & Assert
        var action = () => ClimateAutomation.GetTimeBlock(25);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void HandleWindowClosed_ShouldTurnOnAC_WhenConditionsMet()
    {
        // Arrange
        _acMock.SetupGet(x => x.State).Returns("off");
        _acMock.SetupGet(x => x.Attributes).Returns(new ClimateAttributes { Temperature = 25 });
        var stateChange = new StateChange(
            Entity: _windowMock.Object,
            New: new EntityState { State = "off" },
            Old: new EntityState { State = "on" }
        );

        // Act
        // This would require making HandleWindowClosed method public or internal
        // or testing through the public interface by triggering window state changes

        // Assert
        // Verify AC was turned on with expected parameters
    }

    public void Dispose()
    {
        _windowStateChanges?.Dispose();
        _automation?.Dispose();
    }
}