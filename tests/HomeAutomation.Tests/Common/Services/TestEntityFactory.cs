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

    public static IEnumerable<object[]> EntityTestData =>
        [
            [
                (Func<EntityFactory, string, Entity>)(
                    (f, id) => f.Create<SwitchEntity>("test_device", id)
                ),
                "coffee_machine",
                "switch.test_device_coffee_machine",
            ],
            [
                (Func<EntityFactory, string, Entity>)(
                    (f, id) => f.Create<BinarySensorEntity>("test_device", id)
                ),
                "living_room_motion",
                "binary_sensor.test_device_living_room_motion",
            ],
            [
                (Func<EntityFactory, string, Entity>)(
                    (f, id) => f.Create<LightEntity>("test_device", id)
                ),
                "kitchen_ceiling",
                "light.test_device_kitchen_ceiling",
            ],
            [
                (Func<EntityFactory, string, Entity>)(
                    (f, id) => f.Create<SensorEntity>("test_device", id)
                ),
                "temperature",
                "sensor.test_device_temperature",
            ],
        ];

    [Theory]
    [MemberData(nameof(EntityTestData))]
    public void Should_Create_Entity_With_Correct_EntityId(
        Func<EntityFactory, string, Entity> creator,
        string shortName,
        string expectedEntityId
    )
    {
        var entity = creator(_factory, shortName);

        Assert.NotNull(entity);
        Assert.Equal(expectedEntityId, entity.EntityId);
    }

    [Fact]
    public void Should_Create_Entity_With_And_Without_DeviceName()
    {
        var entity1 = _factory.Create<LightEntity>("kitchen");
        Assert.Equal("light.kitchen", entity1.EntityId);

        var entity2 = _factory.Create<LightEntity>("test_device", "kitchen");
        Assert.Equal("light.test_device_kitchen", entity2.EntityId);
    }

    [Fact]
    public void Should_Create_Entity_When_DeviceName_Is_Null()
    {
        var entity = _factory.Create<LightEntity>(null!, "kitchen");
        Assert.Equal("light.kitchen", entity.EntityId);
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
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _factory.Create<MissingCtorEntity>("test")
        );

        Assert.Contains("No suitable constructor found for MissingCtorEntity", ex.Message);
    }

    [Fact]
    public void Should_Create_NumericSensorEntity_As_Sensor()
    {
        var entity = _factory.Create<NumericSensorEntity>("some_id");
        Assert.Equal("sensor.some_id", entity.EntityId);
        Assert.NotEqual("numeric_sensor.some_id", entity.EntityId);
        Assert.IsType<NumericSensorEntity>(entity);
    }

    private record InvalidType(IHaContext HaContext, string EntityId) : Entity(HaContext, EntityId);

    public record MissingCtorEntity(string SomeOtherParam) : Entity(new MockHaContext(), "dummy");
}
