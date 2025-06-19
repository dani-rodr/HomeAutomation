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
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.OFF);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.DISCONNECTED);
        _mockHaContext.ClearServiceCalls();
    }

    #region Power State Logic Tests

    [Theory]
    [InlineData(HaEntityStates.OFF, HaEntityStates.DISCONNECTED, false)]
    [InlineData(HaEntityStates.ON, HaEntityStates.DISCONNECTED, false)]
    [InlineData(HaEntityStates.OFF, HaEntityStates.CONNECTED, true)]
    [InlineData(HaEntityStates.ON, HaEntityStates.CONNECTED, true)]
    [InlineData(HaEntityStates.ON, HaEntityStates.UNKNOWN, true)]
    [InlineData(HaEntityStates.OFF, HaEntityStates.UNKNOWN, false)]
    public void IsOn_Should_CalculatePowerStateCorrectly(string powerState, string networkState, bool expectedIsOn)
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, powerState);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, networkState);

        // Act
        var isOn = _desktop.IsOn();

        // Assert
        isOn.Should()
            .Be(
                expectedIsOn,
                $"Desktop should be {(expectedIsOn ? "ON" : "OFF")} when power is {powerState} and network is {networkState}"
            );
    }

    [Fact]
    public void IsOn_Should_ReturnFalse_WhenNetworkDisconnected_RegardlessOfPowerState()
    {
        // Arrange - Network disconnected should override power state
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.ON);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.DISCONNECTED);

        // Act
        var isOn = _desktop.IsOn();

        // Assert
        isOn.Should().BeFalse("Desktop should be OFF when network is disconnected, even with power on");
    }

    [Fact]
    public void IsOn_Should_ReturnTrue_WhenNetworkConnected_RegardlessOfPowerState()
    {
        // Arrange - Network connected should make desktop "on" even without power threshold
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.OFF);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.CONNECTED);

        // Act
        var isOn = _desktop.IsOn();

        // Assert
        isOn.Should().BeTrue("Desktop should be ON when network is connected, even without power threshold");
    }

    #endregion

    #region Reactive State Change Tests

    [Fact]
    public void StateChanges_Should_StartWithCurrentState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.ON);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.CONNECTED);
        _stateChanges.Clear();

        // Act - Create new subscription to test StartWith behavior
        var stateChanges = new List<bool>();
        _desktop.StateChanges().Subscribe(state => stateChanges.Add(state));

        // Assert
        stateChanges.Should().HaveCount(1, "Should start with current state");
        stateChanges[0].Should().BeTrue("Current state should be calculated correctly");
    }

    [Fact(Skip = "Temporarily disabled - needs investigation")]
    public void StateChanges_Should_EmitWhenPowerStateChanges()
    {
        // Arrange - Need both sensors to have initial states for CombineLatest to work
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.OFF);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.CONNECTED);
        _stateChanges.Clear();

        // Act - Change power threshold state (network is connected, so desktop should turn ON)
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );

        // Assert
        _stateChanges.Should().HaveCount(1, "Should emit state change for power threshold change");
        _stateChanges[0].Should().BeTrue("Should be ON when power threshold is exceeded and network is connected");
    }

    [Fact(Skip = "Temporarily disabled - needs investigation")]
    public void StateChanges_Should_EmitWhenNetworkStateChanges()
    {
        // Arrange - Need both sensors to have initial states for CombineLatest to work
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.OFF);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.DISCONNECTED);
        _stateChanges.Clear();

        // Act - Change network status (should turn ON even without power threshold)
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.DISCONNECTED,
            HaEntityStates.CONNECTED
        );

        // Assert
        _stateChanges.Should().HaveCount(1, "Should emit state change for network status change");
        _stateChanges[0].Should().BeTrue("Should be ON when network connects");
    }

    [Fact]
    public void StateChanges_Should_CombineMultipleSensorChanges()
    {
        // Arrange
        _stateChanges.Clear();

        // Act - Simulate both sensors changing rapidly
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.DISCONNECTED,
            HaEntityStates.CONNECTED
        );

        // Assert
        _stateChanges.Should().HaveCountGreaterThan(0, "Should emit state changes for combined sensor changes");
        _stateChanges.Last().Should().BeTrue("Final state should be ON with both power and network active");
    }

    [Fact]
    public void StateChanges_Should_UseDistinctUntilChanged()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.ON);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.CONNECTED);
        _stateChanges.Clear();

        // Act - Simulate multiple changes that don't change the calculated state
        _mockHaContext.SimulateStateChange(_entities.PowerPlugThreshold.EntityId, HaEntityStates.ON, HaEntityStates.ON);
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.CONNECTED,
            HaEntityStates.CONNECTED
        );

        // Assert
        _stateChanges.Should().BeEmpty("Should not emit when calculated state doesn't change");
    }

    [Fact]
    public void StateChanges_Should_HandleNetworkDisconnectionCorrectly()
    {
        // Arrange - Need to simulate transitions to get the observable in the right state
        _stateChanges.Clear();

        // First, simulate getting to the starting state
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.DISCONNECTED,
            HaEntityStates.CONNECTED
        );
        _stateChanges.Clear(); // Clear the setup state changes

        // Act - Disconnect network (should force desktop OFF)
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.CONNECTED,
            HaEntityStates.DISCONNECTED
        );

        // Assert
        _stateChanges.Should().HaveCount(1, "Should emit state change when network disconnects");
        _stateChanges[0].Should().BeFalse("Should be OFF when network disconnects, overriding power state");
    }

    [Fact]
    public void StateChanges_Should_HandleComplexStateTransitions()
    {
        // Arrange - Set initial states for both sensors
        _mockHaContext.SetEntityState(_entities.PowerPlugThreshold.EntityId, HaEntityStates.OFF);
        _mockHaContext.SetEntityState(_entities.NetworkStatus.EntityId, HaEntityStates.DISCONNECTED);
        _stateChanges.Clear();

        // Act - Simulate complex state transition sequence
        // 1. Power comes on (but network still disconnected - should stay OFF, no state change)
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );

        // 2. Network connects (should turn ON)
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.DISCONNECTED,
            HaEntityStates.CONNECTED
        );

        // 3. Power goes off (should stay ON due to network, no state change)
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.ON,
            HaEntityStates.OFF
        );

        // 4. Network disconnects (should turn OFF)
        _mockHaContext.SimulateStateChange(
            _entities.NetworkStatus.EntityId,
            HaEntityStates.CONNECTED,
            HaEntityStates.DISCONNECTED
        );

        // Assert - Only changes that actually alter the computed state should emit
        _stateChanges.Should().HaveCount(2, "Should emit state changes only when computed state actually changes");
        _stateChanges[0].Should().BeTrue("Step 2: Should turn ON when network connects");
        _stateChanges[1].Should().BeFalse("Step 4: Should turn OFF when network disconnects");
    }

    #endregion

    #region Power Control Tests

    [Fact]
    public void TurnOn_Should_CallPowerSwitchTurnOn()
    {
        // Act
        _desktop.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOn(_entities.PowerSwitch.EntityId);
    }

    [Fact]
    public void TurnOff_Should_CallPowerSwitchTurnOff()
    {
        // Act
        _desktop.TurnOff();

        // Assert
        _mockHaContext.ShouldHaveCalledSwitchTurnOff(_entities.PowerSwitch.EntityId);
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
        _mockHaContext.SimulateStateChange(
            _entities.PowerPlugThreshold.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );
        _mockHaContext.SimulateStateChange(_entities.RemotePcButton.EntityId, "off", DateTime.Now.ToString());

        // Assert
        _stateChanges.Should().HaveCount(subscriptionCount, "Should not receive new state changes after disposal");

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
        public BinarySensorEntity PowerPlugThreshold { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.smart_plug_1_power_exceeds_threshold");

        public BinarySensorEntity NetworkStatus { get; } =
            new BinarySensorEntity(haContext, "binary_sensor.daniel_pc_network_status");

        public SwitchEntity PowerSwitch { get; } = new SwitchEntity(haContext, "switch.wake_on_lan");

        public InputButtonEntity RemotePcButton { get; } = new InputButtonEntity(haContext, "input_button.remote_pc");
    }
}
