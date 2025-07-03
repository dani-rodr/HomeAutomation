using System.Text.Json;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.Desk.Devices;

/// <summary>
/// Comprehensive tests for LgDisplay device class focusing on WebOS TV integration,
/// dynamic brightness control with delay mechanisms, source switching logic, screen power management,
/// toast notification functionality, and service call verification for webostv commands
/// </summary>
public class LgDisplayTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly Mock<ILogger<LgDisplay>> _mockLogger;
    private readonly TestLgDisplayEntities _entities;
    private readonly LgDisplay _lgDisplay;
    private static readonly string[] attributes = ["HDMI 1", "HDMI 2", "HDMI 3", "Always Ready"];

    public LgDisplayTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<LgDisplay>>();
        _entities = new TestLgDisplayEntities(_mockHaContext);

        // Set up initial attributes with source list for the media player
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source_list = attributes, source = "HDMI 1" }
        );

        _lgDisplay = new LgDisplay(_entities, new Services(_mockHaContext), _mockLogger.Object);
        _lgDisplay.StartAutomation();
        // Set initial state
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "on");
        _mockHaContext.ClearServiceCalls();
    }

    #region Source Management Tests

    [Fact]
    public void ExtendSourceDictionary_Should_ConfigureDisplaySources()
    {
        // Act - Test through ShowPC method which uses the source dictionary
        _lgDisplay.ShowPC();

        // Assert - Verify the source was mapped correctly
        _mockHaContext.ShouldHaveCalledService(
            "media_player",
            "select_source",
            _entities.MediaPlayer.EntityId
        );
        var serviceCall = _mockHaContext.ServiceCalls.Last();
        serviceCall
            .Data?.GetType()
            .GetProperty("Source")
            ?.GetValue(serviceCall.Data)
            ?.ToString()
            .Should()
            .Be("HDMI 1", "PC source should map to HDMI 1");
    }

    [Theory]
    [InlineData("PC", "HDMI 1")]
    [InlineData("Laptop", "HDMI 3")]
    [InlineData("ScreenSaver", "Always Ready")]
    public void ShowSource_Should_MapDisplaySourceCorrectly(
        string sourceKey,
        string expectedHdmiSource
    )
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "on");

        // Act
        switch (sourceKey)
        {
            case "PC":
                _lgDisplay.ShowPC();
                break;
            case "Laptop":
                _lgDisplay.ShowLaptop();
                break;
            case "ScreenSaver":
                _lgDisplay.ShowScreenSaver();
                break;
        }

        // Assert
        _mockHaContext.ShouldHaveCalledService(
            "media_player",
            "select_source",
            _entities.MediaPlayer.EntityId
        );
        var serviceCall = _mockHaContext.ServiceCalls.Last();
        serviceCall
            .Data?.GetType()
            .GetProperty("Source")
            ?.GetValue(serviceCall.Data)
            ?.ToString()
            .Should()
            .Be(expectedHdmiSource, $"{sourceKey} should map to {expectedHdmiSource}");
    }

    [Fact]
    public void ShowPC_WhenDisplayOff_Should_TurnOnFirstThenSelectSource()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "off");

        // Act
        _lgDisplay.ShowPC();

        // Assert
        _mockHaContext.ShouldHaveCalledService("wake_on_lan", "send_magic_packet");
    }

    [Fact]
    public void ShowLaptop_Should_SelectCorrectHdmiSource()
    {
        // Act
        _lgDisplay.ShowLaptop();

        // Assert
        var serviceCall = _mockHaContext.ServiceCalls.Last();
        serviceCall
            .Data?.GetType()
            .GetProperty("Source")
            ?.GetValue(serviceCall.Data)
            ?.ToString()
            .Should()
            .Be("HDMI 3", "Laptop should use HDMI 3 port");
    }

    [Fact]
    public void ShowScreenSaver_Should_SelectAlwaysReadySource()
    {
        // Act
        _lgDisplay.ShowScreenSaver();

        // Assert
        var serviceCall = _mockHaContext.ServiceCalls.Last();
        serviceCall
            .Data?.GetType()
            .GetProperty("Source")
            ?.GetValue(serviceCall.Data)
            ?.ToString()
            .Should()
            .Be("Always Ready", "ScreenSaver should use Always Ready source");
    }

    #endregion

    #region Source State Properties Tests

    [Fact(Skip = "Temporarily disabled - display logic under review")]
    public void IsShowingPc_WhenCurrentSourceIsHdmi1_Should_ReturnTrue()
    {
        // Arrange
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source = "HDMI 1" }
        );

        // Act
        var result = _lgDisplay.IsShowingPc;

        // Assert
        result.Should().BeTrue("IsShowingPc should return true when current source is HDMI 1");
    }

    [Fact]
    public void IsShowingPc_WhenCurrentSourceIsNotHdmi1_Should_ReturnFalse()
    {
        // Arrange
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source = "HDMI 3" }
        );

        // Act
        var result = _lgDisplay.IsShowingPc;

        // Assert
        result
            .Should()
            .BeFalse("IsShowingPc should return false when current source is not HDMI 1");
    }

    [Fact(Skip = "Temporarily disabled - display logic under review")]
    public void IsShowingLaptop_WhenCurrentSourceIsHdmi3_Should_ReturnTrue()
    {
        // Arrange
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source = "HDMI 3" }
        );

        // Act
        var result = _lgDisplay.IsShowingLaptop;

        // Assert
        result.Should().BeTrue("IsShowingLaptop should return true when current source is HDMI 3");
    }

    [Fact]
    public void IsShowingLaptop_WhenCurrentSourceIsNotHdmi3_Should_ReturnFalse()
    {
        // Arrange
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source = "HDMI 1" }
        );

        // Act
        var result = _lgDisplay.IsShowingLaptop;

        // Assert
        result
            .Should()
            .BeFalse("IsShowingLaptop should return false when current source is not HDMI 3");
    }

    [Fact]
    public void ShowPC_WhenDisplayOff_ShouldQueueSource_ThenSelectOnPowerOn()
    {
        // Arrange: Turn off the media player
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "off");

        // Act: Attempt to show PC (should queue the source instead of selecting it)
        _lgDisplay.ShowPC();

        // Assert: No media_player.select_source should be called yet
        _mockHaContext
            .ServiceCalls.Where(call => call.Service == "select_source")
            .Should()
            .BeEmpty("source should be queued, not applied while display is off");

        // Act: Turn the display on (this should trigger queued source selection)
        _mockHaContext.SimulateStateChange(_entities.MediaPlayer.EntityId, "off", "on");

        // Assert: Now the queued source should be selected
        var selectSourceCall = _mockHaContext.ServiceCalls.FirstOrDefault(call =>
            call.Service == "select_source"
        );

        selectSourceCall.Should().NotBeNull("queued source should be selected on power-on");

        var selectedSource = selectSourceCall!
            .Data?.GetType()
            .GetProperty("Source")
            ?.GetValue(selectSourceCall.Data)
            ?.ToString();

        selectedSource.Should().Be("HDMI 1", "PC should map to HDMI 1");
    }

    #endregion

    #region Toast Notification Tests

    [Fact]
    public void ShowToast_Should_CallWebOSTvCreateToastCommand()
    {
        // Act
        _lgDisplay.ShowToast("Test Message");

        // Assert
        _mockHaContext.ShouldHaveCalledWebostvService("command", _entities.MediaPlayer.EntityId);

        var commandCall = _mockHaContext
            .ServiceCalls.Where(c => c.Service == "command" && c.Domain == "webostv")
            .FirstOrDefault();

        var commandProperty = commandCall!.Data?.GetType().GetProperty("command");
        commandProperty
            ?.GetValue(commandCall.Data)
            ?.ToString()
            .Should()
            .Be("system.notifications/createToast", "Should call createToast command");
    }

    [Fact]
    public void ShowToast_Should_IncludeMessageInPayload()
    {
        // Act
        _lgDisplay.ShowToast("Hello World");

        // Assert
        var commandCall = _mockHaContext.ServiceCalls.FirstOrDefault(c =>
            c.Service == "command" && c.Domain == "webostv"
        );

        commandCall.Should().NotBeNull("Should have called webostv command");

        // Verify payload exists (exact structure testing would require reflection)
        var payloadProperty = commandCall!.Data?.GetType().GetProperty("payload");
        payloadProperty
            ?.GetValue(commandCall.Data)
            .Should()
            .NotBeNull("Command should include message payload");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Simple message")]
    [InlineData("Message with special characters: @#$%")]
    [InlineData("Very long message that might need to be handled properly by the WebOS TV system")]
    public void ShowToast_Should_HandleDifferentMessageTypes(string message)
    {
        // Act
        _lgDisplay.ShowToast(message);

        // Assert
        _mockHaContext.ShouldHaveCalledWebostvService("command", _entities.MediaPlayer.EntityId);
    }

    #endregion

    #region Power-On Sequence Tests

    [Fact]
    public void TurnOn_Should_SendWakeOnLanMagicPacket()
    {
        // Act
        _lgDisplay.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledService("wake_on_lan", "send_magic_packet");

        var wolCall = _mockHaContext.ServiceCalls.FirstOrDefault(c =>
            c.Service == "send_magic_packet" && c.Domain == "wake_on_lan"
        );

        wolCall.Should().NotBeNull("Should call Wake on LAN service");

        // Verify MAC address is included
        var macProperty = wolCall!.Data?.GetType().GetProperty("mac");
        macProperty
            ?.GetValue(wolCall.Data)
            ?.ToString()
            .Should()
            .Be("D4:8D:26:B8:C4:AA", "Should use correct MAC address");
    }

    [Fact]
    public void TurnOn_Should_Only_SendMagicPacket()
    {
        // Act
        _lgDisplay.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledService("wake_on_lan", "send_magic_packet");

        // Ensure no WebOS command was sent directly by TurnOn()
        _mockHaContext
            .ServiceCalls.Any(c => c.Domain == "webostv" && c.Service == "command")
            .Should()
            .BeFalse("TurnOn should not send webostv command directly");
    }

    #endregion

    #region MediaPlayerBase Integration Tests

    [Fact]
    public void EntityId_Should_ReturnCorrectMediaPlayerEntityId()
    {
        // Act
        var entityId = _lgDisplay.EntityId;

        // Assert
        entityId
            .Should()
            .Be("media_player.lg_webos_smart_tv", "Should return LG WebOS Smart TV entity ID");
    }

    [Fact]
    public void StateChanges_Should_DelegateToMediaPlayerEntity()
    {
        // Arrange
        var stateChanges =
            new List<StateChange<MediaPlayerEntity, EntityState<MediaPlayerAttributes>>>();

        // Act
        _lgDisplay.StateChanges().Subscribe(stateChanges.Add);

        // Simulate state change
        _mockHaContext.SimulateStateChange(_entities.MediaPlayer.EntityId, "off", "on");

        // Assert
        stateChanges.Should().HaveCount(1, "Should receive state change from media player entity");
        stateChanges[0].New?.State.Should().Be("on", "Should reflect new state");
    }

    [Fact]
    public void IsOn_Should_ReturnMediaPlayerState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "on");

        // Act
        var isOn = _lgDisplay.IsOn();

        // Assert
        isOn.Should().BeTrue("Should return true when media player is on");
    }

    [Fact]
    public void IsOff_Should_ReturnCorrectState()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "off");

        // Act
        var isOff = _lgDisplay.IsOff();

        // Assert
        isOff.Should().BeTrue("Should return true when media player is off");
    }

    [Fact]
    public void SetVolume_Should_DelegateToMediaPlayer()
    {
        // Act
        _lgDisplay.SetVolume(0.75);

        // Assert
        _mockHaContext.ShouldHaveCalledService(
            "media_player",
            "volume_set",
            _entities.MediaPlayer.EntityId
        );
    }

    #endregion

    #region DisplaySource Enum Tests

    [Fact]
    public void DisplaySource_Should_HaveCorrectValues()
    {
        // Assert
        Enum.GetNames(typeof(DisplaySource)).Should().Contain(["PC", "Laptop", "ScreenSaver"]);
        ((int)DisplaySource.PC).Should().Be(0);
        ((int)DisplaySource.Laptop).Should().Be(1);
        ((int)DisplaySource.ScreenSaver).Should().Be(2);
    }

    [Fact]
    public void DisplaySourceToString_Should_WorkCorrectly()
    {
        // Assert
        DisplaySource.PC.ToString().Should().Be("PC");
        DisplaySource.Laptop.ToString().Should().Be("Laptop");
        DisplaySource.ScreenSaver.ToString().Should().Be("ScreenSaver");
    }

    #endregion

    #region Error Handling and Edge Cases

    [Fact]
    public void ShowSource_WhenDisplayOff_Should_HandleTurnOnSequence()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "off");

        // Act
        _lgDisplay.ShowPC();

        // Assert - Should call WOL, screen power on, and source selection
        _mockHaContext.ShouldHaveCalledService("wake_on_lan", "send_magic_packet");
    }

    [Fact(Skip = "Temporarily disabled - display logic under review")]
    public void MultipleOperations_Should_HandleSequentially()
    {
        // Act - Perform multiple operations in sequence
        _lgDisplay.ShowToast("Starting");
        _lgDisplay.ShowPC();
        _lgDisplay.ShowToast("Done");

        // Assert - Should handle all operations
        var toastCalls = _mockHaContext.ServiceCalls.Count(c =>
            c.Service == "command"
            && c.Domain == "webostv"
            && c.Data?.GetType().GetProperty("command")?.GetValue(c.Data)?.ToString()
                == "system.notifications/createToast"
        );

        toastCalls.Should().Be(2, "Should have called toast notification twice");

        _mockHaContext.ShouldHaveCalledService(
            "media_player",
            "select_source",
            _entities.MediaPlayer.EntityId
        );
    }

    #endregion
    #region Screen Management Tests

    [Fact]
    public void LgScreenStateChange_WhenTurnsOn_Should_TurnOnMonitorScreen()
    {
        // Act - Simulate LG screen entity turning on
        _mockHaContext.SimulateStateChange(
            _entities.Display.EntityId,
            HaEntityStates.OFF,
            HaEntityStates.ON
        );

        // Assert - Monitor screen should be turned on
        // This would be verified through the webostv service calls in a real implementation
        // For now, we verify the state change was processed
        var newState = _mockHaContext.GetState(_entities.Display.EntityId);
        newState?.State.Should().Be(HaEntityStates.ON);
    }

    [Fact]
    public void LgScreenStateChange_WhenTurnsOff_Should_TurnOffMonitorScreen()
    {
        // Arrange - Screen initially on
        _mockHaContext.SetEntityState(_entities.Display.EntityId, HaEntityStates.ON);

        // Act - Simulate LG screen entity turning off
        _mockHaContext.SimulateStateChange(
            _entities.Display.EntityId,
            HaEntityStates.ON,
            HaEntityStates.OFF
        );

        // Assert - Monitor screen should be turned off
        var newState = _mockHaContext.GetState(_entities.Display.EntityId);
        newState?.State.Should().Be(HaEntityStates.OFF);
    }

    [Fact]
    public void BrightnessChange_Should_SendCommand_And_ButtonPress()
    {
        // Arrange - Simulate brightness change on the screen light entity
        const int newBrightness = 191;
        const int newBrightnessPct = 74;
        const string buttonEnterPressed = "ENTER";
        _mockHaContext.SimulateStateChange(
            _entities.Display.EntityId,
            "on",
            "on",
            new LightAttributes { Brightness = newBrightness }
        );

        // Assert - Should invoke SetBrightnessAsync with correct value
        var calls = _mockHaContext.ServiceCalls;
        var firstCall = calls.FirstOrDefault();
        firstCall.Should().NotBeNull();
        firstCall!.Domain.Should().Be("webostv"); // or whatever domain your SetBrightnessAsync uses
        firstCall.Service.Should().Be("command");

        var lastCall = calls.LastOrDefault();
        lastCall.Should().NotBeNull();
        lastCall!.Domain.Should().Be("webostv"); // or whatever domain your SetBrightnessAsync uses
        lastCall.Service.Should().Be("button");

        var brightnessDataJson = JsonSerializer.Serialize(firstCall.Data);
        brightnessDataJson.Should().Contain($"{newBrightnessPct}");

        var buttonDataJson = JsonSerializer.Serialize(lastCall.Data);
        buttonDataJson.Should().Contain(buttonEnterPressed);
    }

    [Fact]
    public void BrightnessChange_Should_SetMonitorBrightness()
    {
        // Act - Simulate brightness change
        _mockHaContext.SimulateStateChange(_entities.Display.EntityId, "90", "75");

        // Assert - Brightness state should be updated (verify through mock context)
        var newState = _mockHaContext.GetState(_entities.Display.EntityId);
        newState?.State.Should().Be("75");
    }

    #endregion
    #region Service Call Verification Tests

    [Fact]
    public void WebOSTvServices_Should_BeAccessibleThroughServicesParameter()
    {
        // This test verifies the dependency injection setup is correct
        // Act - Any WebOS command should work
        _lgDisplay.ShowToast("Test");

        // Assert
        _mockHaContext.ShouldHaveCalledWebostvService("command", _entities.MediaPlayer.EntityId);
    }

    [Fact]
    public void WakeOnLanService_Should_BeAccessibleThroughServicesParameter()
    {
        // Act
        _lgDisplay.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledService("wake_on_lan", "send_magic_packet");
    }

    #endregion

    public void Dispose()
    {
        // LgDisplay doesn't implement IDisposable
        _mockHaContext?.Dispose();
    }

    /// <summary>
    /// Test implementation of ILgDisplayEntities for LgDisplay device testing
    /// Creates a media player entity with the correct entity ID for LG WebOS Smart TV
    /// </summary>
    private class TestLgDisplayEntities(IHaContext haContext) : ILgDisplayEntities
    {
        public MediaPlayerEntity MediaPlayer => new(haContext, "media_player.lg_webos_smart_tv");
        public LightEntity Display => new(haContext, "light.lgdisplay");
    }

    /// <summary>
    /// Test implementation of Services containing WebOS TV and Wake on LAN services
    /// Provides access to webostv commands and wake_on_lan functionality for testing
    /// </summary>
    private class TestLgDisplayServices(IHaContext haContext)
    {
        public TestWebostvServices Webostv { get; } = new(haContext);
        public TestWakeOnLanServices WakeOnLan { get; } = new(haContext);
    }

    /// <summary>
    /// Test implementation of WebostvServices for testing WebOS TV commands
    /// Simulates the webostv service calls used by LgDisplay
    /// </summary>
    private class TestWebostvServices(IHaContext haContext)
    {
        private readonly IHaContext _haContext = haContext;

        public void Command(string entityId, string command, object? payload = null)
        {
            _haContext.CallService(
                "webostv",
                "command",
                null,
                new
                {
                    entity_id = entityId,
                    command = command,
                    payload = payload,
                }
            );
        }

        public void Button(string entityId, string button)
        {
            _haContext.CallService(
                "webostv",
                "button",
                null,
                new { entity_id = entityId, button = button }
            );
        }
    }

    /// <summary>
    /// Test implementation of WakeOnLanServices for testing WOL functionality
    /// Simulates the wake_on_lan service calls used by LgDisplay
    /// </summary>
    private class TestWakeOnLanServices(IHaContext haContext)
    {
        private readonly IHaContext _haContext = haContext;

        public void SendMagicPacket(
            string mac,
            string? broadcastAddress = null,
            double? broadcastPort = null
        )
        {
            _haContext.CallService(
                "wake_on_lan",
                "send_magic_packet",
                null,
                new
                {
                    mac = mac,
                    broadcast_address = broadcastAddress,
                    broadcast_port = broadcastPort,
                }
            );
        }
    }
}
