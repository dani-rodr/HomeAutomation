using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class PersonControllerTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly TestEntities _entities;
    private readonly Mock<ILogger<PersonController>> _mockLogger;
    private readonly PersonController _controller;
    private readonly List<string> _arrivedHomeEvents = [];
    private readonly List<string> _leftHomeEvents = [];
    private readonly List<string> _directUnlockEvents = [];

    public PersonControllerTests()
    {
        _mockHaContext = new MockHaContext();
        _mockLogger = new Mock<ILogger<PersonController>>();

        _entities = new TestEntities(_mockHaContext);

        _controller = new PersonController(
            _entities,
            new HomeAssistantGenerated.Services(_mockHaContext),
            _mockLogger.Object
        );
        _controller.StartAutomation();

        // Subscribe to observables to collect events for testing
        _controller.ArrivedHome.Subscribe(_arrivedHomeEvents.Add);
        _controller.LeftHome.Subscribe(_leftHomeEvents.Add);
        _controller.DirectUnlock.Subscribe(_directUnlockEvents.Add);
    }

    [Fact]
    public void SetHome_Should_SetLocationToHome_And_IncrementCounter()
    {
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        _controller.SetHome();

        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterIncrement(_entities.Counter.EntityId);
    }

    [Fact]
    public void SetAway_Should_SetLocationToAway_And_DecrementCounter()
    {
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        _controller.SetAway();

        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterDecrement(_entities.Counter.EntityId);
    }

    [Fact]
    public void ToggleLocation_Should_ChangeWhenPersonIsHome()
    {
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation)
        );
        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterDecrement(_entities.Counter.EntityId);
    }

    [Fact]
    public void ToggleLocation_Should_ChangeWhenPersonIsAway()
    {
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        _mockHaContext.StateChangeSubject.OnNext(
            StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation)
        );
        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterIncrement(_entities.Counter.EntityId);
    }

    #region Observable Tests

    [Fact]
    public void ArrivedHome_PersonIsAway_SingleTrigger_ShouldEmitEntityId()
    {
        // Arrange - Person is away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Home trigger activates
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.HomeTrigger1, "off", "on");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should emit the trigger's entity ID
        Assert.Single(_arrivedHomeEvents);
        Assert.Equal(_entities.HomeTrigger1.EntityId, _arrivedHomeEvents[0]);
    }

    [Fact]
    public void ArrivedHome_PersonIsHome_ShouldNotEmit()
    {
        // Arrange - Person is already home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Home trigger activates
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.HomeTrigger1, "off", "on");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not emit anything
        Assert.Empty(_arrivedHomeEvents);
    }

    [Fact]
    public void ArrivedHome_MultipleTriggers_PersonIsAway_ShouldEmitCorrectEntityIds()
    {
        // Arrange - Person is away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Multiple home triggers activate
        var trigger1Change = StateChangeHelpers.CreateStateChange(
            _entities.HomeTrigger1,
            "off",
            "on"
        );
        var trigger2Change = StateChangeHelpers.CreateStateChange(
            _entities.HomeTrigger2,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(trigger1Change);
        _mockHaContext.StateChangeSubject.OnNext(trigger2Change);

        // Assert - Should emit both trigger entity IDs
        Assert.Equal(2, _arrivedHomeEvents.Count);
        Assert.Contains(_entities.HomeTrigger1.EntityId, _arrivedHomeEvents);
        Assert.Contains(_entities.HomeTrigger2.EntityId, _arrivedHomeEvents);
    }

    [Fact]
    public void ArrivedHome_TriggerTurnsOff_ShouldNotEmit()
    {
        // Arrange - Person is away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Home trigger turns off (not on)
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.HomeTrigger1, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not emit (only responds to "on" state)
        Assert.Empty(_arrivedHomeEvents);
    }

    [Fact]
    public void LeftHome_PersonIsHome_After60Seconds_ShouldEmitEntityId()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Away trigger turns off
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.AwayTrigger1, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Advance time by exactly 60 seconds
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should emit after delay
        Assert.Single(_leftHomeEvents);
        Assert.Equal(_entities.AwayTrigger1.EntityId, _leftHomeEvents[0]);
    }

    [Fact]
    public void LeftHome_PersonIsAway_ShouldNotEmit()
    {
        // Arrange - Person is already away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Away trigger turns off
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.AwayTrigger1, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Advance time by 60 seconds
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should not emit
        Assert.Empty(_leftHomeEvents);
    }

    [Fact]
    public void LeftHome_Before60Seconds_ShouldNotEmit()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Away trigger turns off
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.AwayTrigger1, "on", "off");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Advance time by only 59 seconds (before delay)
        _mockHaContext.AdvanceTimeBySeconds(59);

        // Assert - Should not emit before delay completes
        Assert.Empty(_leftHomeEvents);
    }

    [Fact]
    public void LeftHome_MultipleAwayTriggers_PersonIsHome_ShouldEmitForValidTriggers()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Multiple away triggers turn off at slightly different times
        var trigger1Change = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger1,
            "on",
            "off"
        );
        var trigger2Change = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger2,
            "on",
            "off"
        );

        _mockHaContext.StateChangeSubject.OnNext(trigger1Change);
        _mockHaContext.StateChangeSubject.OnNext(trigger2Change);

        // Advance time by 60 seconds from the second trigger
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Each trigger should emit 2 events
        Assert.True(
            _leftHomeEvents.Count == 2,
            $"Expected at least 2 event, got {_leftHomeEvents.Count}"
        );
        Assert.True(
            _leftHomeEvents.All(id =>
                id == _entities.AwayTrigger1.EntityId || id == _entities.AwayTrigger2.EntityId
            ),
            "All emitted events should be from valid away triggers"
        );
    }

    [Fact]
    public void LeftHome_MultipleTriggersSequential_PersonIsHome_ShouldEmitBoth()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - First trigger turns off, wait for it to complete, then second trigger
        var trigger1Change = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger1,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(trigger1Change);

        // Advance time by 60 seconds for first trigger
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Verify first trigger emitted
        Assert.Single(_leftHomeEvents);
        Assert.Equal(_entities.AwayTrigger1.EntityId, _leftHomeEvents[0]);

        // Second trigger turns off
        var trigger2Change = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger2,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(trigger2Change);

        // Advance time by 60 seconds for second trigger
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should have both trigger entity IDs
        Assert.Equal(2, _leftHomeEvents.Count);
        Assert.Contains(_entities.AwayTrigger1.EntityId, _leftHomeEvents);
        Assert.Contains(_entities.AwayTrigger2.EntityId, _leftHomeEvents);
    }

    [Fact]
    public void LeftHome_TriggerTurnsOn_ShouldNotEmit()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Away trigger turns on (not off)
        var stateChange = StateChangeHelpers.CreateStateChange(_entities.AwayTrigger1, "off", "on");
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Advance time by 60 seconds
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Assert - Should not emit (only responds to "off" state)
        Assert.Empty(_leftHomeEvents);
    }

    [Fact]
    public void DirectUnlock_SingleTrigger_ShouldEmitEntityIdImmediately()
    {
        // Act - Direct unlock trigger activates
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.DirectUnlockTrigger1,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should emit immediately (no delay)
        Assert.Single(_directUnlockEvents);
        Assert.Equal(_entities.DirectUnlockTrigger1.EntityId, _directUnlockEvents[0]);
    }

    [Fact]
    public void DirectUnlock_MultipleTriggers_ShouldEmitAllEntityIds()
    {
        // Act - Multiple direct unlock triggers activate
        var trigger1Change = StateChangeHelpers.CreateStateChange(
            _entities.DirectUnlockTrigger1,
            "off",
            "on"
        );
        var trigger2Change = StateChangeHelpers.CreateStateChange(
            _entities.DirectUnlockTrigger2,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(trigger1Change);
        _mockHaContext.StateChangeSubject.OnNext(trigger2Change);

        // Assert - Should emit both trigger entity IDs
        Assert.Equal(2, _directUnlockEvents.Count);
        Assert.Contains(_entities.DirectUnlockTrigger1.EntityId, _directUnlockEvents);
        Assert.Contains(_entities.DirectUnlockTrigger2.EntityId, _directUnlockEvents);
    }

    [Fact]
    public void DirectUnlock_TriggerTurnsOff_ShouldNotEmit()
    {
        // Act - Direct unlock trigger turns off (not on)
        var stateChange = StateChangeHelpers.CreateStateChange(
            _entities.DirectUnlockTrigger1,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(stateChange);

        // Assert - Should not emit (only responds to "on" state)
        Assert.Empty(_directUnlockEvents);
    }

    [Fact]
    public void Name_NoFriendlyName_ShouldReturnUnknown()
    {
        // Arrange - No friendly name attribute
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act & Assert
        Assert.Equal("Unknown", _controller.Name);
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Act - Dispose should not throw
        var exception = Record.Exception(() => _controller.Dispose());

        // Assert - No exception should be thrown
        Assert.Null(exception);
    }

    #endregion

    #region Subject and Toggle Tests

    [Fact]
    public void ToggleLocation_PersonIsHome_ShouldTriggerLeftHomeSubjectAndSetAway()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - Press toggle button
        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert - Should emit person entity ID via subject and call SetAway
        Assert.Single(_leftHomeEvents);
        Assert.Equal(_entities.Person.EntityId, _leftHomeEvents[0]);
        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterDecrement(_entities.Counter.EntityId);
    }

    [Fact]
    public void ToggleLocation_PersonIsAway_ShouldTriggerArrivedHomeSubjectAndSetHome()
    {
        // Arrange - Person is away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Press toggle button
        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert - Should emit person entity ID via subject and call SetHome
        Assert.Single(_arrivedHomeEvents);
        Assert.Equal(_entities.Person.EntityId, _arrivedHomeEvents[0]);
        _mockHaContext.ShouldHaveCalledService("device_tracker", "see");
        _mockHaContext.ShouldHaveCalledCounterIncrement(_entities.Counter.EntityId);
    }

    [Fact]
    public void ArrivedHome_MergesPhysicalTriggersAndSubjectEvents()
    {
        // Arrange - Person is away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Trigger both physical sensor and toggle button
        var sensorTrigger = StateChangeHelpers.CreateStateChange(
            _entities.HomeTrigger1,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(sensorTrigger);

        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert - Should have both events: sensor entity ID and person entity ID
        Assert.Equal(2, _arrivedHomeEvents.Count);
        Assert.Contains(_entities.HomeTrigger1.EntityId, _arrivedHomeEvents);
        Assert.Contains(_entities.Person.EntityId, _arrivedHomeEvents);
    }

    [Fact]
    public void LeftHome_MergesPhysicalTriggersAndSubjectEvents()
    {
        // Arrange - Person is home
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act - First trigger physical sensor
        var sensorTrigger = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger1,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(sensorTrigger);

        // Advance time by 60 seconds for sensor delay
        _mockHaContext.AdvanceTimeBySeconds(60);

        // Verify sensor event emitted
        Assert.Single(_leftHomeEvents);
        Assert.Equal(_entities.AwayTrigger1.EntityId, _leftHomeEvents[0]);

        // Then trigger toggle button (should emit immediately via subject)
        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert - Should have both events: sensor entity ID and person entity ID
        Assert.Equal(2, _leftHomeEvents.Count);
        Assert.Contains(_entities.AwayTrigger1.EntityId, _leftHomeEvents);
        Assert.Contains(_entities.Person.EntityId, _leftHomeEvents);
    }

    [Fact]
    public void ToggleLocation_MultipleTogglesPresses_ShouldEmitMultipleEvents()
    {
        // Arrange - Person starts away
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Toggle multiple times (away -> home -> away -> home)
        var buttonPress1 = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress1); // Should go home

        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home"); // Update state after SetHome

        var buttonPress2 = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress2); // Should go away

        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home"); // Update state after SetAway

        var buttonPress3 = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress3); // Should go home again

        // Assert - Should have alternating events
        Assert.Equal(2, _arrivedHomeEvents.Count); // Two arrivals (button 1 and 3)
        Assert.Single(_leftHomeEvents); // One departure (button 2)
        Assert.All(_arrivedHomeEvents, eventId => Assert.Equal(_entities.Person.EntityId, eventId));
        Assert.All(_leftHomeEvents, eventId => Assert.Equal(_entities.Person.EntityId, eventId));
    }

    [Fact]
    public void SubjectEventsEmitPersonEntityId_PhysicalTriggersEmitSensorEntityId()
    {
        // Arrange - Person is away for arrived test, home for left test
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act & Assert - ArrivedHome: Physical trigger emits sensor ID
        var homeSensorTrigger = StateChangeHelpers.CreateStateChange(
            _entities.HomeTrigger1,
            "off",
            "on"
        );
        _mockHaContext.StateChangeSubject.OnNext(homeSensorTrigger);

        Assert.Single(_arrivedHomeEvents);
        Assert.Equal(_entities.HomeTrigger1.EntityId, _arrivedHomeEvents[0]);

        // Change person to home for left test
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "home");

        // Act & Assert - LeftHome: Physical trigger emits sensor ID after delay
        var awaySensorTrigger = StateChangeHelpers.CreateStateChange(
            _entities.AwayTrigger1,
            "on",
            "off"
        );
        _mockHaContext.StateChangeSubject.OnNext(awaySensorTrigger);
        _mockHaContext.AdvanceTimeBySeconds(60);

        Assert.Single(_leftHomeEvents);
        Assert.Equal(_entities.AwayTrigger1.EntityId, _leftHomeEvents[0]);

        // Act & Assert - Toggle emits person entity ID
        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        Assert.Equal(2, _leftHomeEvents.Count);
        Assert.Equal(_entities.Person.EntityId, _leftHomeEvents[1]); // Second event should be person ID
    }

    [Fact]
    public void SubjectsAfterDisposal_ShouldNotEmitEvents()
    {
        // Arrange - Person is away, set up initial state
        _mockHaContext.SetEntityState(_entities.Person.EntityId, "not_home");

        // Act - Dispose the controller
        _controller.Dispose();

        // Clear any existing events
        _arrivedHomeEvents.Clear();
        _leftHomeEvents.Clear();

        // Try to trigger toggle button after disposal
        var buttonPress = StateChangeHelpers.CreateButtonPress(_entities.ToggleLocation);
        _mockHaContext.StateChangeSubject.OnNext(buttonPress);

        // Assert - Should not emit any events after disposal
        Assert.Empty(_arrivedHomeEvents);
        Assert.Empty(_leftHomeEvents);
    }

    #endregion

    public void Dispose()
    {
        _controller.Dispose();
        _mockHaContext.Dispose();
    }

    private class TestEntities(IHaContext context) : IPersonEntities
    {
        public PersonEntity Person => new(context, "person.test_person");
        public CounterEntity Counter => new(context, "counter.test_home_counter");
        public ButtonEntity ToggleLocation => new(context, "button.test_toggle");

        // Multiple triggers for comprehensive testing
        public BinarySensorEntity HomeTrigger1 => new(context, "binary_sensor.home_trigger_1");
        public BinarySensorEntity HomeTrigger2 => new(context, "binary_sensor.home_trigger_2");
        public BinarySensorEntity AwayTrigger1 => new(context, "binary_sensor.away_trigger_1");
        public BinarySensorEntity AwayTrigger2 => new(context, "binary_sensor.away_trigger_2");
        public BinarySensorEntity DirectUnlockTrigger1 =>
            new(context, "binary_sensor.direct_unlock_1");
        public BinarySensorEntity DirectUnlockTrigger2 =>
            new(context, "binary_sensor.direct_unlock_2");

        public IEnumerable<BinarySensorEntity> HomeTriggers => [HomeTrigger1, HomeTrigger2];
        public IEnumerable<BinarySensorEntity> AwayTriggers => [AwayTrigger1, AwayTrigger2];
        public IEnumerable<BinarySensorEntity> DirectUnlockTriggers =>
            [DirectUnlockTrigger1, DirectUnlockTrigger2];
    }
}
