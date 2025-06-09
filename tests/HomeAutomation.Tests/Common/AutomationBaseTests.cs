using HomeAutomation.Common;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;

namespace HomeAutomation.Tests.Common;

public class AutomationBaseTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<InputBooleanEntity> _masterSwitchMock;
    private readonly TestableAutomationBase _automation;

    public AutomationBaseTests()
    {
        _loggerMock = new Mock<ILogger>();
        _masterSwitchMock = new Mock<InputBooleanEntity>();
        _automation = new TestableAutomationBase(_loggerMock.Object, _masterSwitchMock.Object);
    }

    [Fact]
    public void Constructor_WithMasterSwitch_SetsPropertiesCorrectly()
    {
        // Assert
        _automation.Logger.Should().Be(_loggerMock.Object);
        _automation.MasterSwitch.Should().Be(_masterSwitchMock.Object);
    }

    [Fact]
    public void Constructor_WithoutMasterSwitch_SetsLoggerOnly()
    {
        // Arrange & Act
        var automation = new TestableAutomationBase(_loggerMock.Object);

        // Assert
        automation.Logger.Should().Be(_loggerMock.Object);
        automation.MasterSwitch.Should().BeNull();
    }

    [Fact]
    public void StartAutomation_WithMasterSwitch_SubscribesToStateChanges()
    {
        // Arrange
        _masterSwitchMock.SetupGet(x => x.State).Returns("on");

        // Act
        _automation.StartAutomation();

        // Assert
        _automation.IsAutomationEnabled.Should().BeTrue();
        _automation.TestGetSwitchableAutomationsCalled.Should().BeTrue();
    }

    [Fact]
    public void StartAutomation_WithoutMasterSwitch_CallsGetSwitchableAutomations()
    {
        // Arrange
        var automation = new TestableAutomationBase(_loggerMock.Object);

        // Act
        automation.StartAutomation();

        // Assert
        automation.TestGetSwitchableAutomationsCalled.Should().BeTrue();
    }

    [Fact]
    public void RestartAutomations_DisposesAndRestartsSubscriptions()
    {
        // Arrange
        _masterSwitchMock.SetupGet(x => x.State).Returns("on");
        _automation.StartAutomation();
        var initialDisposableCount = _automation.TestDisposableCount;

        // Act
        _automation.RestartAutomations();

        // Assert
        _automation.TestDisposableCount.Should().Be(initialDisposableCount);
        _automation.TestGetSwitchableAutomationsCalled.Should().BeTrue();
    }

    [Fact]
    public void Dispose_DisposesAllSubscriptions()
    {
        // Arrange
        _masterSwitchMock.SetupGet(x => x.State).Returns("on");
        _automation.StartAutomation();

        // Act
        _automation.Dispose();

        // Assert
        _automation.TestIsDisposed.Should().BeTrue();
    }

    private class TestableAutomationBase : AutomationBase
    {
        public bool TestGetSwitchableAutomationsCalled { get; private set; }
        public int TestDisposableCount => _disposables.Count;
        public bool TestIsDisposed { get; private set; }

        public TestableAutomationBase(ILogger logger, InputBooleanEntity? masterSwitch = null) 
            : base(logger, masterSwitch)
        {
        }

        protected override IEnumerable<IDisposable> GetSwitchableAutomations()
        {
            TestGetSwitchableAutomationsCalled = true;
            yield return new Mock<IDisposable>().Object;
        }

        public override void Dispose()
        {
            TestIsDisposed = true;
            base.Dispose();
        }
    }
}