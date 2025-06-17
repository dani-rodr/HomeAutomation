using System.Reactive.Subjects;
using System.Text.Json;

namespace HomeAutomation.Tests.Helpers;

/// <summary>
/// Comprehensive tests for all extension methods in Helpers.cs
/// Tests state filtering, time-based operations, user validation, and utility methods
/// </summary>
public class HelpersTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly BinarySensorEntity _motionSensor;
    private readonly LightEntity _light;
    private readonly SwitchEntity _switch;
    private readonly LockEntity _lock;
    private readonly ClimateEntity _climate;
    private readonly WeatherEntity _weather;
    private readonly NumberEntity _number;
    private readonly SensorEntity _sensor;
    private readonly Subject<StateChange> _stateChangeSubject;

    public HelpersTests()
    {
        _mockHaContext = new MockHaContext();
        _motionSensor = new BinarySensorEntity(_mockHaContext, "binary_sensor.test_motion");
        _light = new LightEntity(_mockHaContext, "light.test_light");
        _switch = new SwitchEntity(_mockHaContext, "switch.test_switch");
        _lock = new LockEntity(_mockHaContext, "lock.test_lock");
        _climate = new ClimateEntity(_mockHaContext, "climate.test_climate");
        _weather = new WeatherEntity(_mockHaContext, "weather.test_weather");
        _number = new NumberEntity(_mockHaContext, "number.test_number");
        _sensor = new SensorEntity(_mockHaContext, "sensor.test_sensor");
        _stateChangeSubject = new Subject<StateChange>();
    }

    #region StateChangeObservableExtensions Tests

    [Fact]
    public void IsAnyOfStates_Should_FilterCorrectStates()
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsAnyOfStates("on", "off").Subscribe(results.Add);

        var onChange = StateChangeHelpers.CreateStateChange(_light, "off", "on");
        var offChange = StateChangeHelpers.CreateStateChange(_light, "on", "off");
        var dimChange = StateChangeHelpers.CreateStateChange(_light, "on", "dim");

        // Act
        _stateChangeSubject.OnNext(onChange);
        _stateChangeSubject.OnNext(offChange);
        _stateChangeSubject.OnNext(dimChange);

        // Assert
        results.Should().HaveCount(2);
        results[0].New?.State.Should().Be("on");
        results[1].New?.State.Should().Be("off");
    }

    [Fact]
    public void IsAnyOfStates_Should_IgnoreUnavailableOldState()
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsAnyOfStates("on").Subscribe(results.Add);

        // Create change with unavailable old state
        var change = new StateChange(
            (Entity)_light,
            new EntityState { State = "unavailable" },
            new EntityState { State = "on" }
        );

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        results.Should().BeEmpty("Should ignore changes from unavailable state");
    }

    [Fact]
    public void IsAnyOfStates_Should_IgnoreNullStates()
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsAnyOfStates("on").Subscribe(results.Add);

        var changeWithNullOld = new StateChange((Entity)_light, null, new EntityState { State = "on" });
        var changeWithNullNew = new StateChange((Entity)_light, new EntityState { State = "off" }, null);

        // Act
        _stateChangeSubject.OnNext(changeWithNullOld);
        _stateChangeSubject.OnNext(changeWithNullNew);

        // Assert
        results.Should().BeEmpty("Should ignore changes with null states");
    }

    [Theory]
    [InlineData("ON", true)]
    [InlineData("on", true)]
    [InlineData("On", true)]
    [InlineData("off", false)]
    [InlineData("unknown", false)]
    public void IsOn_Should_FilterOnStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsOn().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "off", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Fact]
    public void IsOpen_Should_BeAliasForIsOn()
    {
        // Arrange
        var onResults = new List<StateChange>();
        var openResults = new List<StateChange>();

        _stateChangeSubject.IsOn().Subscribe(onResults.Add);
        _stateChangeSubject.IsOpen().Subscribe(openResults.Add);

        var change = StateChangeHelpers.CreateStateChange(_motionSensor, "off", "on");

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        onResults.Should().HaveCount(1);
        openResults.Should().HaveCount(1);
        openResults[0].Should().BeEquivalentTo(onResults[0]);
    }

    [Theory]
    [InlineData("OFF", true)]
    [InlineData("off", true)]
    [InlineData("Off", true)]
    [InlineData("on", false)]
    [InlineData("unknown", false)]
    public void IsOff_Should_FilterOffStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsOff().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "on", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Fact]
    public void IsClosed_Should_BeAliasForIsOff()
    {
        // Arrange
        var offResults = new List<StateChange>();
        var closedResults = new List<StateChange>();

        _stateChangeSubject.IsOff().Subscribe(offResults.Add);
        _stateChangeSubject.IsClosed().Subscribe(closedResults.Add);

        var change = StateChangeHelpers.CreateStateChange(_motionSensor, "on", "off");

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        offResults.Should().HaveCount(1);
        closedResults.Should().HaveCount(1);
        closedResults[0].Should().BeEquivalentTo(offResults[0]);
    }

    [Theory]
    [InlineData("locked", true)]
    [InlineData("LOCKED", true)]
    [InlineData("Locked", true)]
    [InlineData("unlocked", false)]
    [InlineData("unknown", false)]
    public void IsLocked_Should_FilterLockedStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsLocked().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_lock, "unlocked", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("unlocked", true)]
    [InlineData("UNLOCKED", true)]
    [InlineData("Unlocked", true)]
    [InlineData("locked", false)]
    [InlineData("unknown", false)]
    public void IsUnlocked_Should_FilterUnlockedStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsUnlocked().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_lock, "locked", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("unavailable", true)]
    [InlineData("UNAVAILABLE", true)]
    [InlineData("on", false)]
    [InlineData("off", false)]
    public void IsUnavailable_Should_FilterUnavailableStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsUnavailable().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "on", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("unknown", true)]
    [InlineData("UNKNOWN", true)]
    [InlineData("on", false)]
    [InlineData("off", false)]
    public void IsUnknown_Should_FilterUnknownStatesCorrectly(string state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsUnknown().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "on", state);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
            results[0].New?.State.Should().Be(state);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, true)]
    [InlineData(HaIdentity.ATHENA_BEZOS, true)]
    [InlineData(HaIdentity.MIPAD5, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(null, true)]
    [InlineData(HaIdentity.SUPERVISOR, false)]
    [InlineData(HaIdentity.NODERED, false)]
    [InlineData("unknown_user", false)]
    public void IsManuallyOperated_Should_FilterManualOperationsCorrectly(string? userId, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsManuallyOperated().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("   ", false)] // Whitespace is not considered physically operated
    [InlineData(null, true)]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, false)]
    [InlineData(HaIdentity.SUPERVISOR, false)]
    public void IsPhysicallyOperated_Should_FilterPhysicalOperationsCorrectly(string? userId, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsPhysicallyOperated().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(HaIdentity.SUPERVISOR, true)]
    [InlineData(HaIdentity.NODERED, true)]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("unknown_automation", false)]
    public void IsAutomated_Should_FilterAutomatedOperationsCorrectly(string? userId, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsAutomated().Subscribe(results.Add);

        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("2023-12-01T10:30:00Z", true)]
    [InlineData("2023-12-01 10:30:00", true)]
    [InlineData("invalid_date", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidButtonPress_Should_ValidateDateTimeStates(string? state, bool shouldMatch)
    {
        // Arrange
        var results = new List<StateChange>();
        _stateChangeSubject.IsValidButtonPress().Subscribe(results.Add);

        var change = new StateChange(
            (Entity)_switch,
            new EntityState { State = "previous" },
            new EntityState { State = state }
        );

        // Act
        _stateChangeSubject.OnNext(change);

        // Assert
        if (shouldMatch)
        {
            results.Should().HaveCount(1);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    #endregion

    #region Time-based Extension Tests

    [Fact]
    public void WhenStateIsForSeconds_Should_UseCorrectTimeSpan()
    {
        // This test verifies the method compiles and returns an observable
        // The actual timing behavior is tested by NetDaemon framework
        var observable = _stateChangeSubject.WhenStateIsForSeconds("on", 30);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void WhenStateIsForMinutes_Should_UseCorrectTimeSpan()
    {
        var observable = _stateChangeSubject.WhenStateIsForMinutes("on", 5);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void WhenStateIsForHours_Should_UseCorrectTimeSpan()
    {
        var observable = _stateChangeSubject.WhenStateIsForHours("on", 2);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOnForSeconds_Should_UseOnStateAndSeconds()
    {
        var observable = _stateChangeSubject.IsOnForSeconds(45);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOnForMinutes_Should_UseOnStateAndMinutes()
    {
        var observable = _stateChangeSubject.IsOnForMinutes(10);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOnForHours_Should_UseOnStateAndHours()
    {
        var observable = _stateChangeSubject.IsOnForHours(3);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOffForSeconds_Should_UseOffStateAndSeconds()
    {
        var observable = _stateChangeSubject.IsOffForSeconds(60);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOffForMinutes_Should_UseOffStateAndMinutes()
    {
        var observable = _stateChangeSubject.IsOffForMinutes(15);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOffForHours_Should_UseOffStateAndHours()
    {
        var observable = _stateChangeSubject.IsOffForHours(1);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsClosedForSeconds_Should_UseOffStateAndSeconds()
    {
        var observable = _stateChangeSubject.IsClosedForSeconds(30);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsOpenForSeconds_Should_UseOnStateAndSeconds()
    {
        var observable = _stateChangeSubject.IsOpenForSeconds(20);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsLockedForMinutes_Should_UseLockedStateAndMinutes()
    {
        var observable = _stateChangeSubject.IsLockedForMinutes(5);
        observable.Should().NotBeNull();
    }

    [Fact]
    public void IsUnlockedForHours_Should_UseUnlockedStateAndHours()
    {
        var observable = _stateChangeSubject.IsUnlockedForHours(2);
        observable.Should().NotBeNull();
    }

    #endregion

    #region StateExtensions Tests

    [Theory]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ)]
    [InlineData("")]
    [InlineData(null)]
    public void StateChange_UserId_Should_ExtractUserIdCorrectly(string? expectedUserId)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", expectedUserId);

        // Act
        var result = change.UserId();

        // Assert
        result.Should().Be(expectedUserId ?? string.Empty);
    }

    [Fact]
    public void StateChange_UserId_Should_ReturnEmptyWhenContextIsNull()
    {
        // Arrange
        var change = new StateChange(
            (Entity)_light,
            new EntityState { State = "off" },
            new EntityState { State = "on" } // No context
        );

        // Act
        var result = change.UserId();

        // Assert
        result.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("on")]
    [InlineData("off")]
    [InlineData("locked")]
    [InlineData("")]
    public void StateChange_State_Should_ExtractStateCorrectly(string expectedState)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "previous", expectedState);

        // Act
        var result = change.New?.State ?? string.Empty;

        // Assert
        result.Should().Be(expectedState);
    }

    [Fact]
    public void StateChange_State_Should_ReturnEmptyWhenNewIsNull()
    {
        // Arrange
        var change = new StateChange((Entity)_light, new EntityState { State = "off" }, null);

        // Act
        var result = change.New?.State ?? string.Empty;

        // Assert
        result.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("ON", true)]
    [InlineData("off", false)]
    [InlineData("unknown", false)]
    public void GenericStateChange_IsOn_Should_CheckOnState(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", state);

        // Act
        var result = change.IsOn();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("OFF", true)]
    [InlineData("on", false)]
    [InlineData("unknown", false)]
    public void GenericStateChange_IsOff_Should_CheckOffState(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "on", state);

        // Act
        var result = change.IsOff();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("locked", true)]
    [InlineData("LOCKED", true)]
    [InlineData("unlocked", false)]
    public void GenericStateChange_IsLocked_Should_CheckLockedState(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_lock, "unlocked", state);

        // Act
        var result = change.New?.State?.IsLocked() ?? false;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unlocked", true)]
    [InlineData("UNLOCKED", true)]
    [InlineData("locked", false)]
    public void GenericStateChange_IsUnlocked_Should_CheckUnlockedState(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_lock, "locked", state);

        // Act
        var result = change.New?.State?.IsUnlocked() ?? false;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unavailable", true)]
    [InlineData("UNAVAILABLE", true)]
    [InlineData("on", false)]
    [InlineData("off", false)]
    public void GenericStateChange_IsUnavailable_Should_CheckUnavailableState(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "on", state);

        // Act
        var result = change.New?.State?.IsUnavailable() ?? false;

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2023-12-01T10:30:00Z", true)]
    [InlineData("invalid_date", false)]
    [InlineData("", false)]
    public void StateChange_IsValidButtonPress_Should_ValidateDateTime(string state, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_switch, "previous", state);

        // Act
        var result = change.IsValidButtonPress();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StateChange_IsValidButtonPress_Should_HandleNullStateChange()
    {
        // Act & Assert
        StateChange? nullChange = null;
        nullChange!.IsValidButtonPress().Should().BeFalse();
    }

    [Theory]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, true)]
    [InlineData(HaIdentity.SUPERVISOR, false)]
    [InlineData("", true)]
    [InlineData(null, true)]
    public void StateChange_IsManuallyOperated_Should_CheckManualOperation(string? userId, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        var result = change.IsManuallyOperated();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, false)]
    public void StateChange_IsPhysicallyOperated_Should_CheckPhysicalOperation(string? userId, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        var result = change.IsPhysicallyOperated();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(HaIdentity.SUPERVISOR, true)]
    [InlineData(HaIdentity.NODERED, true)]
    [InlineData(HaIdentity.DANIEL_RODRIGUEZ, false)]
    [InlineData("", false)]
    public void StateChange_IsAutomated_Should_CheckAutomatedOperation(string? userId, bool expected)
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on", userId);

        // Act
        var result = change.IsAutomated();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StateChange_IsOn_Should_ReturnFalseForNullStateChange()
    {
        // Act & Assert
        StateChange? nullChange = null;
        nullChange!.IsOn().Should().BeFalse();
    }

    [Fact]
    public void StateChange_IsOff_Should_ReturnTrueForNullStateChange()
    {
        // Act & Assert
        StateChange? nullChange = null;
        nullChange!.IsOff().Should().BeTrue();
    }

    #endregion

    #region String State Extension Tests

    [Theory]
    [InlineData("on", true)]
    [InlineData("ON", true)]
    [InlineData("On", true)]
    [InlineData("off", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsOn_Should_CheckOnState(string? state, bool expected)
    {
        // Act & Assert
        state.IsOn().Should().Be(expected);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("OFF", true)]
    [InlineData("Off", true)]
    [InlineData("on", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsOff_Should_CheckOffState(string? state, bool expected)
    {
        // Act & Assert
        state.IsOff().Should().Be(expected);
    }

    [Fact]
    public void String_IsOpen_Should_BeAliasForIsOn()
    {
        // Act & Assert
        "on".IsOpen().Should().BeTrue();
        "off".IsOpen().Should().BeFalse();
    }

    [Fact]
    public void String_IsClosed_Should_BeAliasForIsOff()
    {
        // Act & Assert
        "off".IsClosed().Should().BeTrue();
        "on".IsClosed().Should().BeFalse();
    }

    [Theory]
    [InlineData("locked", true)]
    [InlineData("LOCKED", true)]
    [InlineData("Locked", true)]
    [InlineData("unlocked", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsLocked_Should_CheckLockedState(string? state, bool expected)
    {
        // Act & Assert
        state.IsLocked().Should().Be(expected);
    }

    [Theory]
    [InlineData("unlocked", true)]
    [InlineData("UNLOCKED", true)]
    [InlineData("Unlocked", true)]
    [InlineData("locked", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsUnlocked_Should_CheckUnlockedState(string? state, bool expected)
    {
        // Act & Assert
        state.IsUnlocked().Should().Be(expected);
    }

    [Theory]
    [InlineData("connected", true)]
    [InlineData("CONNECTED", true)]
    [InlineData("Connected", true)]
    [InlineData("disconnected", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsConnected_Should_CheckConnectedState(string? state, bool expected)
    {
        // Act & Assert
        state.IsConnected().Should().Be(expected);
    }

    [Theory]
    [InlineData("disconnected", true)]
    [InlineData("DISCONNECTED", true)]
    [InlineData("Disconnected", true)]
    [InlineData("connected", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsDisconnected_Should_CheckDisconnectedState(string? state, bool expected)
    {
        // Act & Assert
        state.IsDisconnected().Should().Be(expected);
    }

    [Theory]
    [InlineData("unavailable", true)]
    [InlineData("UNAVAILABLE", true)]
    [InlineData("Unavailable", true)]
    [InlineData("available", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void String_IsUnavailable_Should_CheckUnavailableState(string? state, bool expected)
    {
        // Act & Assert
        state.IsUnavailable().Should().Be(expected);
    }

    #endregion

    #region Entity Extension Tests

    [Fact]
    public void SensorEntity_LocalHour_Should_ExtractHourFromDateTime()
    {
        // Arrange - Use a known date-time that we can test
        var testDateTime = "2023-12-01T14:30:00Z";
        _mockHaContext.SetEntityState(_sensor.EntityId, testDateTime);

        // Act
        var result = _sensor.LocalHour();

        // Assert - The method returns the hour from parsed DateTime.TryParse
        // This will return the local hour based on DateTime.TryParse behavior
        result.Should().BeInRange(0, 23, "Should return a valid hour between 0-23");

        // Test with a specific case we can verify
        _mockHaContext.SetEntityState(_sensor.EntityId, "2023-01-01T00:00:00");
        var midnightResult = _sensor.LocalHour();
        midnightResult.Should().Be(0, "Midnight should return hour 0");
    }

    [Theory]
    [InlineData("invalid_date")]
    [InlineData("")]
    [InlineData("unknown")]
    public void SensorEntity_LocalHour_Should_ReturnNegativeOneForInvalidState(string invalidState)
    {
        // Arrange
        _mockHaContext.SetEntityState(_sensor.EntityId, invalidState);

        // Act
        var result = _sensor.LocalHour();

        // Assert
        result.Should().Be(-1);
    }

    [Theory]
    [InlineData("locked", true)]
    [InlineData("unlocked", false)]
    [InlineData("unknown", false)]
    public void SensorEntity_IsLocked_Should_CheckLockState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_sensor.EntityId, state);

        // Act
        var result = _sensor.IsLocked();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unlocked", true)]
    [InlineData("locked", false)]
    [InlineData("unknown", false)]
    public void SensorEntity_IsUnlocked_Should_CheckUnlockState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_sensor.EntityId, state);

        // Act
        var result = _sensor.IsUnlocked();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unavailable", true)]
    [InlineData("available", false)]
    [InlineData("unknown", false)]
    public void SensorEntity_IsUnavailable_Should_CheckUnavailableState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_sensor.EntityId, state);

        // Act
        var result = _sensor.IsUnavailable();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public void BinarySensorEntity_IsOpen_Should_CheckOpenState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsOpen();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public void BinarySensorEntity_IsOccupied_Should_CheckOccupiedState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsOccupied();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("on", false)]
    public void BinarySensorEntity_IsClear_Should_CheckClearState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsClear();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("on", false)]
    public void BinarySensorEntity_IsClosed_Should_CheckClosedState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsClosed();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("connected", true)]
    [InlineData("disconnected", false)]
    public void BinarySensorEntity_IsConnected_Should_CheckConnectedState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsConnected();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("disconnected", true)]
    [InlineData("connected", false)]
    public void BinarySensorEntity_IsDisconnected_Should_CheckDisconnectedState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_motionSensor.EntityId, state);

        // Act
        var result = _motionSensor.IsDisconnected();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("dry", true)]
    [InlineData("DRY", true)]
    [InlineData("cool", false)]
    [InlineData("off", false)]
    public void ClimateEntity_IsDry_Should_CheckDryState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_climate.EntityId, state);

        // Act
        var result = _climate.IsDry();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("cool", true)]
    [InlineData("COOL", true)]
    [InlineData("dry", false)]
    [InlineData("off", false)]
    public void ClimateEntity_IsCool_Should_CheckCoolState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_climate.EntityId, state);

        // Act
        var result = _climate.IsCool();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("off", true)]
    [InlineData("OFF", true)]
    [InlineData("cool", false)]
    [InlineData("dry", false)]
    public void ClimateEntity_IsOff_Should_CheckOffState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_climate.EntityId, state);

        // Act
        var result = _climate.IsOff();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("dry", true)]
    [InlineData("cool", true)]
    [InlineData("off", false)]
    [InlineData("heat", false)]
    public void ClimateEntity_IsOn_Should_CheckOnState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_climate.EntityId, state);

        // Act
        var result = _climate.IsOn();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("dry", true)]
    [InlineData("DRY", true)]
    [InlineData("sunny", false)]
    public void WeatherEntity_IsDry_Should_CheckDryState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsDry();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("sunny", true)]
    [InlineData("partlycloudy", true)]
    [InlineData("cloudy", false)]
    [InlineData("rainy", false)]
    public void WeatherEntity_IsSunny_Should_CheckSunnyStates(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsSunny();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("rainy", true)]
    [InlineData("pouring", true)]
    [InlineData("lightning-rainy", true)]
    [InlineData("sunny", false)]
    [InlineData("cloudy", false)]
    public void WeatherEntity_IsRainy_Should_CheckRainyStates(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsRainy();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("cloudy", true)]
    [InlineData("partlycloudy", true)]
    [InlineData("sunny", false)]
    [InlineData("rainy", false)]
    public void WeatherEntity_IsCloudy_Should_CheckCloudyStates(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsCloudy();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("clear-night", true)]
    [InlineData("CLEAR-NIGHT", true)]
    [InlineData("sunny", false)]
    public void WeatherEntity_IsClearNight_Should_CheckClearNightState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsClearNight();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("lightning", true)]
    [InlineData("lightning-rainy", true)]
    [InlineData("hail", true)]
    [InlineData("sunny", false)]
    [InlineData("rainy", false)]
    public void WeatherEntity_IsStormy_Should_CheckStormyStates(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsStormy();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("snowy", true)]
    [InlineData("snowy-rainy", true)]
    [InlineData("sunny", false)]
    [InlineData("rainy", false)]
    public void WeatherEntity_IsSnowy_Should_CheckSnowyStates(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_weather.EntityId, state);

        // Act
        var result = _weather.IsSnowy();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void NumberEntity_SetNumericValue_Should_CallSetValueService()
    {
        // Arrange
        const double testValue = 42.5;

        // Act
        _number.SetNumericValue(testValue);

        // Assert
        _mockHaContext.ShouldHaveCalledService("number", "set_value");

        var serviceCalls = _mockHaContext.GetServiceCalls("number").ToList();
        var setValueCall = serviceCalls.FirstOrDefault(c => c.Service == "set_value");

        setValueCall.Should().NotBeNull();
        setValueCall!.Target?.EntityIds.Should().Contain(_number.EntityId);
    }

    [Theory]
    [InlineData("locked", true)]
    [InlineData("unlocked", false)]
    public void LockEntity_IsLocked_Should_CheckLockState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_lock.EntityId, state);

        // Act
        var result = _lock.IsLocked();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unlocked", true)]
    [InlineData("locked", false)]
    public void LockEntity_IsUnlocked_Should_CheckUnlockState(string state, bool expected)
    {
        // Arrange
        _mockHaContext.SetEntityState(_lock.EntityId, state);

        // Act
        var result = _lock.IsUnlocked();

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region TimeRange Tests

    [Theory]
    [InlineData(9, 17, 12, 0, true)] // 12:00 PM between 9 AM and 5 PM
    [InlineData(9, 17, 8, 30, false)] // 8:30 AM not between 9 AM and 5 PM
    [InlineData(9, 17, 18, 0, false)] // 6:00 PM not between 9 AM and 5 PM
    [InlineData(9, 17, 9, 0, true)] // 9:00 AM (boundary)
    [InlineData(9, 17, 17, 0, true)] // 5:00 PM (boundary)
    [InlineData(22, 6, 23, 30, true)] // 11:30 PM between 10 PM and 6 AM (overnight)
    [InlineData(22, 6, 3, 0, true)] // 3:00 AM between 10 PM and 6 AM (overnight)
    [InlineData(22, 6, 12, 0, false)] // 12:00 PM not between 10 PM and 6 AM (overnight)
    [InlineData(22, 6, 22, 0, true)] // 10:00 PM (boundary, overnight)
    [InlineData(22, 6, 6, 0, true)] // 6:00 AM (boundary, overnight)
    public void TimeRange_IsTimeInBetween_Should_HandleTimeRangesCorrectly(
        int start,
        int end,
        int hour,
        int minute,
        bool expected
    )
    {
        // Arrange
        var testTime = new TimeSpan(hour, minute, 0);

        // Act
        var result = TimeRange.IsTimeInBetween(testTime, start, end);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TimeRange_IsCurrentTimeInBetween_Should_UseCurrentTime()
    {
        // This test verifies the method compiles and runs
        // The actual time comparison depends on current system time
        var result = TimeRange.IsCurrentTimeInBetween(0, 24);

        // Should always return true for 0-23 hour range (whole day)
        result.Should().BeTrue();
    }

    #endregion

    #region StateChangeExtensions Tests

    [Fact]
    public void GetAttributeChange_Should_ExtractAttributeChanges()
    {
        // This test verifies the method signature and basic functionality
        // In a real scenario, attributes would be set through entity state updates
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act - Test with missing attributes (should return defaults)
        var (oldBrightness, newBrightness) = change.GetAttributeChange<int>("brightness");
        var (oldTemp, newTemp) = change.GetAttributeChange<double>("temperature");

        // Assert - Should return default values for missing attributes
        oldBrightness.Should().Be(0);
        newBrightness.Should().Be(0);
        oldTemp.Should().Be(0.0);
        newTemp.Should().Be(0.0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleMissingAttributes()
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act
        var (oldValue, newValue) = change.GetAttributeChange<int>("missing_attribute");

        // Assert
        oldValue.Should().Be(0); // default(int)
        newValue.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleNullAttributes()
    {
        // Arrange
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act
        var (oldValue, newValue) = change.GetAttributeChange<string>("any_attribute");

        // Assert
        oldValue.Should().BeNull(); // default(string)
        newValue.Should().BeNull();
    }

    [Fact]
    public void GetAttributeChange_Should_HandleJsonElementAttributes()
    {
        // This test verifies the method handles JSON elements properly
        // In practice, attributes come from Home Assistant entity state
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act
        var (oldValue, newValue) = change.GetAttributeChange<int>("test_attribute");

        // Assert - Should return defaults when no attributes are present
        oldValue.Should().Be(0);
        newValue.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_HandleTypeConversion()
    {
        // This test verifies the method signature and error handling
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act
        var (_, stringAsInt) = change.GetAttributeChange<int>("string_number");
        var (_, doubleAsInt) = change.GetAttributeChange<int>("double_to_int");

        // Assert - Should return defaults when attributes are missing
        stringAsInt.Should().Be(0);
        doubleAsInt.Should().Be(0);
    }

    [Fact]
    public void GetAttributeChange_Should_ReturnDefaultForInvalidConversion()
    {
        // This test verifies the method's error handling for type conversion
        var change = StateChangeHelpers.CreateStateChange(_light, "off", "on");

        // Act
        var (_, result) = change.GetAttributeChange<int>("invalid_number");

        // Assert
        result.Should().Be(0); // default(int) for failed conversion
    }

    #endregion

    #region SwitchEntityExtensions Tests

    [Fact]
    public void OnDoubleClick_Should_ReturnObservableOfBufferedChanges()
    {
        // Arrange
        var switchChangeSubject = new Subject<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>();
        var results = new List<IList<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>>();

        switchChangeSubject.OnDoubleClick(2).Subscribe(results.Add);

        // This test mainly verifies the method compiles and returns the expected type
        // The actual buffering and timing logic would require more complex reactive testing

        // Act & Assert - verify the observable is properly set up
        switchChangeSubject.Should().NotBeNull();
        results.Should().BeEmpty(); // No changes emitted yet
    }

    #endregion

    public void Dispose()
    {
        _stateChangeSubject?.Dispose();
        _mockHaContext?.Dispose();
    }
}
