using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class TestEntityFactory
{
    private readonly MockHaContext _mockHaContext = new();
    private readonly Mock<ILogger<EntityFactory>> _mockLogger = new();
    private readonly EntityFactory _factory;

    public TestEntityFactory()
    {
        _factory = new EntityFactory(_mockHaContext, _mockLogger.Object);
    }

    [Theory]
    [InlineData(typeof(SwitchEntity), "coffee_machine", "switch.coffee_machine")]
    [InlineData(
        typeof(BinarySensorEntity),
        "living_room_motion",
        "binary_sensor.living_room_motion"
    )]
    [InlineData(typeof(LightEntity), "kitchen_ceiling", "light.kitchen_ceiling")]
    [InlineData(typeof(SensorEntity), "temperature", "sensor.temperature")]
    public void Should_Create_Entity_With_Correct_EntityId(
        Type entityType,
        string shortName,
        string expectedEntityId
    )
    {
        var method = typeof(EntityFactory)
            .GetMethod(nameof(EntityFactory.Create))!
            .MakeGenericMethod(entityType);
        var entity = (Entity)method.Invoke(_factory, [shortName])!;

        Assert.NotNull(entity);
        Assert.Equal(expectedEntityId, entity.EntityId);
    }

    [Fact]
    public void Should_Create_BinarySensorEntity_With_Correct_EntityId()
    {
        var shortName = "living_room_motion";

        var entity = _factory.Create<BinarySensorEntity>(shortName);

        Assert.NotNull(entity);
        Assert.Equal("binary_sensor.living_room_motion", entity.EntityId);
    }

    [Fact]
    public void Should_Log_Error_When_Type_Is_Invalid()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.Create<InvalidType>("some_id")
        );

        Assert.Contains("Unexpected entity type name", ex.Message);
    }

    [Fact]
    public void Should_Throw_When_Entity_Missing_Required_Constructor()
    {
        // Arrange
        var shortName = "test";

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.Create<MissingCtorEntity>(shortName)
        );

        Assert.Contains("No suitable constructor found for MissingCtorEntity", ex.Message);
    }

    private record InvalidType(IHaContext HaContext, string EntityId) : Entity(HaContext, EntityId);

    public record MissingCtorEntity(string SomeOtherParam) : Entity(new MockHaContext(), "dummy");
}
