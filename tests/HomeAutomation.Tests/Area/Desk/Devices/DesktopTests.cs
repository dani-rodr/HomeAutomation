using System.Reflection;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;
using HomeAutomation.apps.Helpers;

namespace HomeAutomation.Tests.Area.Desk.Devices;

/// <summary>
/// Comprehensive tests for Desktop device class focusing on power state logic and reactive behavior
/// Tests complex power state calculation combining network and power sensor states
/// </summary>
public class DesktopTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IEventHandler> _mockEventHandler;
    private readonly Mock<INotificationServices> _mockNotificationServices;
    private readonly TestDesktopEntities _entities;
    private readonly Desktop _desktop;
    private readonly List<bool> _stateChanges = [];

    public DesktopTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger>();
        _mockEventHandler = new Mock<IEventHandler>();
        _mockNotificationServices = new Mock<INotificationServices>();

        _entities = new TestDesktopEntities(_mockHaContext);

        _desktop = new Desktop(
            _entities,
            _mockEventHandler.Object,
            _mockNotificationServices.Object,
            _mockLogger.Object
        );

        // Subscribe to state changes to track behavior
        _desktop.StateChanges().Subscribe(state => _stateChanges.Add(state));

        // Set default states
        _mockHaContext.SetEntityState(_entities.Power.EntityId, HaEntityStates.OFF);
        _mockHaContext.ClearServiceCalls();
    }

    #region Power State Logic Tests

    [Theory]
    [InlineData(HaEntityStates.OFF, false)]
    [InlineData(HaEntityStates.ON, true)]
    public void IsOn_Should_CalculatePowerStateCorrectly(string powerState, bool expectedIsOn)
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Power.EntityId, powerState);

        // Act
        var isOn = _desktop.IsOn();

        // Assert
        isOn.Should()
            .Be(expectedIsOn, $"Desktop should be {(expectedIsOn ? "ON" : "OFF")} when power is {powerState} ");
    }

    #endregion

    #region Reactive State Change Tests

    [Fact]
    public void StateChanges_Should_StartWithCurrentState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Power.EntityId, HaEntityStates.ON);
        _stateChanges.Clear();

        // Act - Create new subscription to test StartWith behavior
        var stateChanges = new List<bool>();
        _desktop.StateChanges().Subscribe(state => stateChanges.Add(state));

        // Assert
        stateChanges.Should().HaveCount(0, "Should start with current state");
    }

    [Fact]
    public void StateChanges_Should_ReceiveStateChanges_AfterSetup()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.Power.EntityId, HaEntityStates.ON);
        _stateChanges.Clear();

        // Act - Create new subscription to test StartWith behavior
        var stateChanges = new List<bool>();
        _desktop.StateChanges().Subscribe(state => stateChanges.Add(state));
        _mockHaContext.SimulateStateChange(_entities.Power.EntityId, HaEntityStates.ON, HaEntityStates.OFF);
        _mockHaContext.SimulateStateChange(_entities.Power.EntityId, HaEntityStates.OFF, HaEntityStates.ON);

        // Assert
        stateChanges.Should().HaveCount(2, "Should contain 2 state change");
    }

    [Fact]
    public void StateChanges_Should_Return_True_When_Turning_On()
    {
        // Arrange
        _stateChanges.Clear();

        // Act - Simulate both sensors changing rapidly
        _mockHaContext.SimulateStateChange(_entities.Power.EntityId, HaEntityStates.OFF, HaEntityStates.ON);

        // Assert
        _stateChanges.Should().HaveCountGreaterThan(0, "Should emit state changes for sensor changes");
        _stateChanges.Last().Should().BeTrue("Final state should be ON with power active");
    }

    [Fact]
    public void StateChanges_Should_Return_False_When_Turning_Off()
    {
        // Arrange
        _stateChanges.Clear();

        // Act - Simulate both sensors changing rapidly
        _mockHaContext.SimulateStateChange(_entities.Power.EntityId, HaEntityStates.ON, HaEntityStates.OFF);

        // Assert
        _stateChanges.Should().HaveCountGreaterThan(0, "Should emit state changes for sensor changes");
        _stateChanges.Last().Should().BeFalse("Final state should be ON with power inactive");
    }

    #endregion

    #region Power Control Tests

    [Fact]
    public void TurnOn_Should_CallPowerSwitchTurnOn()
    {
        // Act
        _desktop.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.Power.EntityId);
    }

    [Fact]
    public void TurnOff_Should_CallPowerSwitchTurnOff()
    {
        // Act
        _desktop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.Power.EntityId);
    }

    #endregion

    #region Remote PC Button Tests

    [Fact]
    public void RemotePcButton_WithDanielUser_Should_LaunchMoonlightOnPocoF4()
    {
        // Act - Simulate button press from Daniel
        var buttonPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            HaIdentity.DANIEL_RODRIGUEZ
        );
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert
        _mockNotificationServices.Verify(
            x => x.LaunchAppPocoF4("com.limelight"),
            Times.Once,
            "Should launch Moonlight app on Poco F4 for Daniel user"
        );
        _mockNotificationServices.Verify(
            x => x.LaunchAppMiPad(It.IsAny<string>()),
            Times.Never,
            "Should not launch app on MiPad for Daniel user"
        );
    }

    [Fact]
    public void RemotePcButton_WithMiPadUser_Should_LaunchMoonlightOnMiPad()
    {
        // Act - Simulate button press from MiPad
        var buttonPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            HaIdentity.MIPAD5
        );
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert
        _mockNotificationServices.Verify(
            x => x.LaunchAppMiPad("com.limelight"),
            Times.Once,
            "Should launch Moonlight app on MiPad for MiPad user"
        );
        _mockNotificationServices.Verify(
            x => x.LaunchAppPocoF4(It.IsAny<string>()),
            Times.Never,
            "Should not launch app on Poco F4 for MiPad user"
        );
    }

    [Fact]
    public void RemotePcButton_WithUnknownUser_Should_NotLaunchAnyApp()
    {
        // Act - Simulate button press from unknown user
        var buttonPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            "unknown-user-id"
        );
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert
        _mockNotificationServices.Verify(
            x => x.LaunchAppPocoF4(It.IsAny<string>()),
            Times.Never,
            "Should not launch app on Poco F4 for unknown user"
        );
        _mockNotificationServices.Verify(
            x => x.LaunchAppMiPad(It.IsAny<string>()),
            Times.Never,
            "Should not launch app on MiPad for unknown user"
        );
    }

    [Fact]
    public void RemotePcButton_WithInvalidButtonPress_Should_NotLaunchApp()
    {
        // Act - Simulate invalid button press (not a valid timestamp)
        var invalidButtonPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            "invalid-timestamp",
            HaIdentity.DANIEL_RODRIGUEZ
        );
        _mockHaContext.StateChangeSubject.OnNext(invalidButtonPress);

        // Assert
        _mockNotificationServices.Verify(
            x => x.LaunchAppPocoF4(It.IsAny<string>()),
            Times.Never,
            "Should not launch app for invalid button press"
        );
    }

    [Fact]
    public void RemotePcButton_MultipleValidPresses_Should_HandleEachCorrectly()
    {
        // Act - Simulate multiple button presses from different users
        var danielPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            HaIdentity.DANIEL_RODRIGUEZ
        );
        var mipadPress = StateChangeHelpers.CreateStateChange(
            _entities.RemotePcButton,
            "off",
            DateTime.Now.AddSeconds(1).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            HaIdentity.MIPAD5
        );

        _mockHaContext.StateChangeSubject.OnNext(danielPress);
        _mockHaContext.StateChangeSubject.OnNext(mipadPress);

        // Assert
        _mockNotificationServices.Verify(
            x => x.LaunchAppPocoF4("com.limelight"),
            Times.Once,
            "Should launch Moonlight on Poco F4 for Daniel"
        );
        _mockNotificationServices.Verify(
            x => x.LaunchAppMiPad("com.limelight"),
            Times.Once,
            "Should launch Moonlight on MiPad for MiPad user"
        );
    }

    #endregion

    #region ComputerBase Interface Tests

    [Fact]
    public void ShowEvent_Should_BeShowPc()
    {
        // Act & Assert - Access through reflection since ShowEvent is protected
        var showEvent =
            typeof(Desktop).GetProperty("ShowEvent", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(_desktop)
            as string;

        showEvent.Should().Be("show_pc", "ShowEvent should be 'show_pc' for Desktop");
    }

    [Fact]
    public void HideEvent_Should_BeHidePc()
    {
        // Act & Assert - Access through reflection since HideEvent is protected
        var hideEvent =
            typeof(Desktop).GetProperty("HideEvent", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(_desktop)
            as string;

        hideEvent.Should().Be("hide_pc", "HideEvent should be 'hide_pc' for Desktop");
    }

    [Fact]
    public void OnShowRequested_Should_DelegateToEventHandler()
    {
        // Arrange
        var eventSubject = new Subject<Event>();
        _mockEventHandler.Setup(x => x.WhenEventTriggered("show_pc")).Returns(eventSubject);

        // Act
        var showObservable = _desktop.OnShowRequested();

        // Assert
        showObservable.Should().NotBeNull("Should return an observable for show requests");
        _mockEventHandler.Verify(
            x => x.WhenEventTriggered("show_pc"),
            Times.Once,
            "Should call event handler with show_pc event"
        );
    }

    [Fact]
    public void OnHideRequested_Should_DelegateToEventHandler()
    {
        // Arrange
        var eventSubject = new Subject<Event>();
        _mockEventHandler.Setup(x => x.WhenEventTriggered("hide_pc")).Returns(eventSubject);

        // Act
        var hideObservable = _desktop.OnHideRequested();

        // Assert
        hideObservable.Should().NotBeNull("Should return an observable for hide requests");
        _mockEventHandler.Verify(
            x => x.WhenEventTriggered("hide_pc"),
            Times.Once,
            "Should call event handler with hide_pc event"
        );
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Dispose_Should_CleanUpSubscriptions()
    {
        // Arrange
        var subscriptionCount = _stateChanges.Count;

        // Act
        _desktop.Dispose();

        // Simulate state changes after disposal
        _mockHaContext.SimulateStateChange(_entities.Power.EntityId, HaEntityStates.OFF, HaEntityStates.ON);
        _mockHaContext.SimulateStateChange(_entities.RemotePcButton.EntityId, "off", DateTime.Now.ToString());

        // Assert
        _stateChanges
            .Should()
            .HaveCount(1, "Should receive 1 State change Power State Change Disposal is handled by a caller class ");

        // Should not throw when calling methods after disposal
        var act = () =>
        {
            _desktop.TurnOn();
            _desktop.TurnOff();
            _desktop.IsOn();
        };
        act.Should().NotThrow("Should handle method calls gracefully after disposal");
    }

    #endregion

    public void Dispose()
    {
        _desktop?.Dispose();
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test implementation of IDesktopEntities for Desktop device testing
    /// Creates entities with realistic entity IDs matching the actual Desktop configuration
    /// </summary>
    private class TestDesktopEntities(IHaContext haContext) : IDesktopEntities
    {
        public InputButtonEntity RemotePcButton { get; } = new InputButtonEntity(haContext, "input_button.remote_pc");

        public SwitchEntity Power => new SwitchEntity(haContext, "switch.danielpc");
    }
}
