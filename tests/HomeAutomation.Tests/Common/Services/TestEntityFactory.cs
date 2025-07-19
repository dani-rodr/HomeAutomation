using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class TestEntityFactory
{
    private readonly MockHaContext _mockHaContext = new();
    private readonly Mock<ILogger<EntityFactory>> _mockLogger = new();
    private readonly EntityFactory _factory;

    public TestEntityFactory()
    {
        _factory = new EntityFactory(_mockHaContext);
    }

    public static IEnumerable<object[]> EntityTestData =>
        [
            [
                (Func<EntityFactory, string, Entity>)((f, id) => f.Create<SwitchEntity>(id)),
                "coffee_machine",
                "switch.test_device_coffee_machine",
            ],
            [
                (Func<EntityFactory, string, Entity>)((f, id) => f.Create<BinarySensorEntity>(id)),
                "living_room_motion",
                "binary_sensor.test_device_living_room_motion",
            ],
            [
                (Func<EntityFactory, string, Entity>)((f, id) => f.Create<LightEntity>(id)),
                "kitchen_ceiling",
                "light.test_device_kitchen_ceiling",
            ],
            [
                (Func<EntityFactory, string, Entity>)((f, id) => f.Create<SensorEntity>(id)),
                "temperature",
                "sensor.test_device_temperature",
            ],
        ];

    [Theory]
    [MemberData(nameof(EntityTestData))]
    public void Should_Create_Entity_With_Correct_EntityId_When_DeviceName_Set(
        Func<EntityFactory, string, Entity> creator,
        string shortName,
        string expectedEntityId
    )
    {
        _factory.DeviceName = "test_device";
        var entity = creator(_factory, shortName);

        Assert.NotNull(entity);
        Assert.Equal(expectedEntityId, entity.EntityId);
    }

    [Fact]
    public void Should_Set_DeviceName_When_Creatining_Entity_WithDeviceName()
    {
        var entity = _factory.Create<LightEntity>("kitchen");
        Assert.Equal("light.kitchen", entity.EntityId);
        Assert.Equal(string.Empty, _factory.DeviceName);

        entity = _factory.Create<LightEntity>("test_device", "kitchen");
        Assert.Equal("light.test_device_kitchen", entity.EntityId);
        Assert.Equal("test_device", _factory.DeviceName);
    }

    [Fact]
    public void Should_Create_Entity_Without_Prefix_When_DeviceName_Is_Empty()
    {
        var entity = _factory.Create<LightEntity>("kitchen");
        Assert.Equal("light.kitchen", entity.EntityId);
    }

    [Fact]
    public void Should_Create_Entity_Without_Prefix_When_DeviceName_Is_Null()
    {
        _factory.DeviceName = null!;

        var entity = _factory.Create<LightEntity>("kitchen");
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

    private record InvalidType(IHaContext HaContext, string EntityId) : Entity(HaContext, EntityId);

    public record MissingCtorEntity(string SomeOtherParam) : Entity(new MockHaContext(), "dummy");
}
