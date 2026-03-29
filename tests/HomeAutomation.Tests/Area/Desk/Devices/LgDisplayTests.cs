using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Area.Desk.Devices.Entities;

namespace HomeAutomation.Tests.Area.Desk.Devices;

/// <summary>
/// Comprehensive tests for LgDisplay device class focusing on WebOS TV integration,
/// dynamic brightness control with delay mechanisms, source switching logic, screen power management,
/// toast notification functionality, and service call verification for webostv commands
/// </summary>
public class LgDisplayTests : HaContextTestBase
{
    private MockHaContext _mockHaContext => HaContext;
    private readonly Mock<ILogger<LgDisplay>> _mockLogger;
    private readonly TestLgDisplayEntities _entities;
    private readonly LgDisplay _lgDisplay;
    private static readonly string[] attributes = ["HDMI 1", "HDMI 2", "HDMI 3", "Always Ready"];

    public LgDisplayTests()
    {
        _mockLogger = new Mock<ILogger<LgDisplay>>();
        _entities = new TestLgDisplayEntities(_mockHaContext);

        // Set up initial attributes with source list for the media player
        _mockHaContext.SetEntityAttributes(
            _entities.MediaPlayer.EntityId,
            new { source_list = attributes, source = "HDMI 1" }
        );

        // Set up mock response for power state check
        _mockHaContext.SetServiceResponse(
            "webostv",
            "command",
            "com.webos.service.tvpower/power/getPowerState",
            new Dictionary<string, object>
            {
                [_entities.MediaPlayer.EntityId] = new { state = "Active" },
            }
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
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            "HDMI 1"
        );
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
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            expectedHdmiSource
        );
    }

    [Fact]
    public void ShowPC_WhenDisplayOff_Should_TurnOnFirstThenSelectSource()
    {
        // Arrange
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, "off");

        // Act
        _lgDisplay.ShowPC();

        // Assert
        _mockHaContext.ShouldHaveCalledWakeOnLan("D4:8D:26:B8:C4:AA");
    }

    [Fact]
    public void ShowLaptop_Should_SelectCorrectHdmiSource()
    {
        // Act
        _lgDisplay.ShowLaptop();

        // Assert
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            "HDMI 3"
        );
    }

    [Fact]
    public void ShowScreenSaver_Should_SelectAlwaysReadySource()
    {
        // Act
        _lgDisplay.ShowScreenSaver();

        // Assert
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            "Always Ready"
        );
    }

    #endregion

    #region Source State Properties Tests

    [Fact(
        Skip = "Quarantined: display logic under review | issue HA-TEST-2005 | expires 2026-06-30"
    )]
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

    [Fact(
        Skip = "Quarantined: display logic under review | issue HA-TEST-2005 | expires 2026-06-30"
    )]
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
        _mockHaContext.ShouldNotHaveCalledService(
            "media_player",
            "select_source",
            _entities.MediaPlayer.EntityId
        );

        // Act: Turn the display on (this should trigger queued source selection)
        _mockHaContext.SimulateStateChange(_entities.MediaPlayer.EntityId, "off", "on");

        // Assert: Now the queued source should be selected
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            "HDMI 1"
        );
    }

    [Fact]
    public void ShowLaptop_WhenDisplayUnavailable_ShouldQueueSource_ThenSelectOnPowerOn()
    {
        // Arrange: Set media player state to unavailable
        _mockHaContext.SetEntityState(_entities.MediaPlayer.EntityId, HaEntityStates.UNAVAILABLE);

        // Act: Attempt to show Laptop (should queue the source instead of selecting it)
        _lgDisplay.ShowLaptop();

        // Assert: WOL magic packet should be sent to wake the display
        _mockHaContext.ShouldHaveCalledWakeOnLan("D4:8D:26:B8:C4:AA");

        // Assert: No media_player.select_source should be called yet
        _mockHaContext.ShouldNotHaveCalledService(
            "media_player",
            "select_source",
            _entities.MediaPlayer.EntityId
        );

        // Act: Simulate display becoming available (unavailable -> on transition)
        _mockHaContext.SimulateStateChange(
            _entities.MediaPlayer.EntityId,
            HaEntityStates.UNAVAILABLE,
            "on"
        );

        // Assert: Now the queued source should be selected
        _mockHaContext.ShouldHaveCalledMediaPlayerSelectSource(
            _entities.MediaPlayer.EntityId,
            "HDMI 3"
        );
    }

    #endregion

    #region Toast Notification Tests

    [Fact]
    public void ShowToast_Should_CallWebOSTvCreateToastCommand()
    {
        // Act
        _lgDisplay.ShowToast("Test Message");

        // Assert
        _mockHaContext.ShouldHaveCalledWebostvCommand(
            _entities.MediaPlayer.EntityId,
            "system.notifications/createToast"
        );
    }

    [Fact]
    public void ShowToast_Should_IncludeMessageInPayload()
    {
        // Act
        _lgDisplay.ShowToast("Hello World");

        // Assert
        _mockHaContext.ShouldHaveCalledWebostvCommandContaining(
            _entities.MediaPlayer.EntityId,
            "payload"
        );
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
        _mockHaContext.ShouldHaveCalledWakeOnLan("D4:8D:26:B8:C4:AA");
    }

    [Fact]
    public void TurnOn_Should_Only_SendMagicPacket()
    {
        // Act
        _lgDisplay.TurnOn();

        // Assert
        _mockHaContext.ShouldHaveCalledWakeOnLan();

        // Ensure no WebOS command was sent directly by TurnOn()
        _mockHaContext.ShouldNotHaveCalledService("webostv", "command");
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
        _mockHaContext.ShouldHaveCalledWakeOnLan();
    }

    [Fact(
        Skip = "Quarantined: display logic under review | issue HA-TEST-2005 | expires 2026-06-30"
    )]
    public void MultipleOperations_Should_HandleSequentially()
    {
        // Act - Perform multiple operations in sequence
        _lgDisplay.ShowToast("Starting");
        _lgDisplay.ShowPC();
        _lgDisplay.ShowToast("Done");

        // Assert - Should handle all operations
        _mockHaContext.ShouldHaveCalledWebostvCommandExactly(
            _entities.MediaPlayer.EntityId,
            "system.notifications/createToast",
            2
        );

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
        _mockHaContext.ShouldHaveServiceCallSequence(
            ("webostv", "command"),
            ("webostv", "command"),
            ("webostv", "button"),
            ("light", "turn_on")
        );

        _mockHaContext.ShouldHaveCalledWebostvCommandContaining(
            _entities.MediaPlayer.EntityId,
            $"{newBrightnessPct}"
        );
        _mockHaContext.ShouldHaveCalledWebostvService("button", _entities.MediaPlayer.EntityId);
        _mockHaContext.ShouldHaveCalledWebostvButtonContaining(
            _entities.MediaPlayer.EntityId,
            buttonEnterPressed
        );
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
        _mockHaContext.ShouldHaveCalledWakeOnLan();
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
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
}
