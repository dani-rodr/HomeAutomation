using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class PersonControllerTests : IDisposable
{
    private readonly MockHaContext _mockHaContext;
    private readonly TestEntities _entities;
    private readonly Mock<IServices> _mockServices;
    private readonly Mock<ILogger<PersonController>> _mockLogger;
    private readonly PersonController _controller;

    public PersonControllerTests()
    {
        _mockHaContext = new MockHaContext();
        _mockServices = new Mock<IServices>();
        _mockLogger = new Mock<ILogger<PersonController>>();

        _entities = new TestEntities(_mockHaContext);

        _controller = new PersonController(
            _entities,
            new HomeAssistantGenerated.Services(_mockHaContext)
        );
        _controller.StartAutomation();
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

    public void Dispose()
    {
        _controller.Dispose();
        _mockHaContext.Dispose();
    }

    private class TestEntities : IPersonEntities
    {
        public PersonEntity Person { get; }
        public CounterEntity Counter { get; }
        public ButtonEntity ToggleLocation { get; }

        public TestEntities(IHaContext context)
        {
            Person = new PersonEntity(context, "person.test_person");
            Counter = new CounterEntity(context, "counter.test_home_counter");
            ToggleLocation = new ButtonEntity(context, "button.test_toggle");
        }
    }
}
