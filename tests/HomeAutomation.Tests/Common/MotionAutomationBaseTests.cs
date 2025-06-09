using HomeAutomation.Common;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;

namespace HomeAutomation.Tests.Common;

public class MotionAutomationBaseTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<BinarySensorEntity> _motionSensorMock;
    private readonly Mock<NumberEntity> _motionDelaySensorMock;
    private readonly Subject<StateChange> _motionStateChanges;
    private readonly TestableMotionAutomation _automation;

    public MotionAutomationBaseTests()
    {
        _loggerMock = new Mock<ILogger>();
        _motionSensorMock = new Mock<BinarySensorEntity>();
        _motionDelaySensorMock = new Mock<NumberEntity>();
        _motionStateChanges = new Subject<StateChange>();
        
        _motionSensorMock.Setup(x => x.StateChanges()).Returns(_motionStateChanges);
        _motionDelaySensorMock.SetupGet(x => x.State).Returns(60.0);
        
        _automation = new TestableMotionAutomation(
            _loggerMock.Object,
            _motionSensorMock.Object,
            _motionDelaySensorMock.Object);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        // Assert
        _automation.MotionSensor.Should().Be(_motionSensorMock.Object);
        _automation.MotionSensorDelay.Should().Be(_motionDelaySensorMock.Object);
    }

    [Fact]
    public void GetTimeOnMinutes_WithValidDelayState_ReturnsCorrectValue()
    {
        // Arrange
        _motionDelaySensorMock.SetupGet(x => x.State).Returns(120.0);

        // Act
        var result = _automation.GetTimeOnMinutes();

        // Assert
        result.Should().Be(2); // 120 seconds = 2 minutes
    }

    [Fact]
    public void GetTimeOnMinutes_WithNullDelayState_ReturnsDefault()
    {
        // Arrange
        _motionDelaySensorMock.SetupGet(x => x.State).Returns((double?)null);

        // Act
        var result = _automation.GetTimeOnMinutes();

        // Assert
        result.Should().Be(5); // Default value
    }

    [Fact]
    public void GetTimeOnMinutes_WithZeroDelayState_ReturnsDefault()
    {
        // Arrange
        _motionDelaySensorMock.SetupGet(x => x.State).Returns(0.0);

        // Act
        var result = _automation.GetTimeOnMinutes();

        // Assert
        result.Should().Be(5); // Default value
    }

    [Fact]
    public void ShouldTurnOffEntity_ByDefault_ReturnsTrue()
    {
        // Arrange
        var entityMock = new Mock<LightEntity>();

        // Act
        var result = _automation.ShouldTurnOffEntity(entityMock.Object);

        // Assert
        result.Should().BeTrue();
    }

    private class TestableMotionAutomation : MotionAutomationBase
    {
        public TestableMotionAutomation(
            ILogger logger,
            BinarySensorEntity motionSensor,
            NumberEntity motionSensorDelay,
            InputBooleanEntity? masterSwitch = null)
            : base(logger, motionSensor, motionSensorDelay, masterSwitch)
        {
        }

        protected override IEnumerable<IDisposable> GetSwitchableAutomations()
        {
            yield break;
        }
    }
}