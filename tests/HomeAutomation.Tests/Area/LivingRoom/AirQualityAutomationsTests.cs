using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.LivingRoom;

public class AirQualityAutomationsTests
{
    private readonly Mock<IAirQualityEntities> _mockEntities;
    private readonly Mock<ILogger<AirQualityAutomations>> _mockLogger;
    private readonly Mock<SwitchEntity> _mockCleanAirSwitch;
    private readonly Mock<BinarySensorEntity> _mockPresenceSensor;
    private readonly Mock<SwitchEntity> _mockAirPurifierFan;
    private readonly Mock<SwitchEntity> _mockSupportingFan;
    private readonly Mock<NumericSensorEntity> _mockPm25Sensor;
    private readonly Mock<SwitchEntity> _mockLedStatus;

    public AirQualityAutomationsTests()
    {
        _mockEntities = new Mock<IAirQualityEntities>();
        _mockLogger = new Mock<ILogger<AirQualityAutomations>>();

        // Setup entity mocks
        _mockCleanAirSwitch = new Mock<SwitchEntity>();
        _mockPresenceSensor = new Mock<BinarySensorEntity>();
        _mockAirPurifierFan = new Mock<SwitchEntity>();
        _mockSupportingFan = new Mock<SwitchEntity>();
        _mockPm25Sensor = new Mock<NumericSensorEntity>();
        _mockLedStatus = new Mock<SwitchEntity>();

        // Setup entity container to return mocked entities
        _mockEntities.Setup(x => x.CleanAirSwitch).Returns(_mockCleanAirSwitch.Object);
        _mockEntities.Setup(x => x.PresenceSensor).Returns(_mockPresenceSensor.Object);
        _mockEntities.Setup(x => x.AirPurifierFan).Returns(_mockAirPurifierFan.Object);
        _mockEntities.Setup(x => x.SupportingFan).Returns(_mockSupportingFan.Object);
        _mockEntities.Setup(x => x.Pm25Sensor).Returns(_mockPm25Sensor.Object);
        _mockEntities.Setup(x => x.LedStatus).Returns(_mockLedStatus.Object);
    }

    [Fact]
    public void Constructor_Should_UseEntityContainer_Successfully()
    {
        // Act - This tests that the container pattern works for dependency injection
        var automation = new AirQualityAutomations(_mockEntities.Object, _mockLogger.Object);

        // Assert - Constructor should complete without errors
        automation.Should().NotBeNull();

        // Verify that the entity container was accessed during construction
        _mockEntities.Verify(x => x.CleanAirSwitch, Times.AtLeastOnce);
        _mockEntities.Verify(x => x.PresenceSensor, Times.AtLeastOnce);
        _mockEntities.Verify(x => x.AirPurifierFan, Times.AtLeastOnce);
        _mockEntities.Verify(x => x.SupportingFan, Times.AtLeastOnce);
    }

    [Fact]
    public void EntityContainer_Should_SimplifyTesting_ComparedToIndividualParameters()
    {
        // This test demonstrates the advantage of container pattern for testing

        // Before: Would need to mock 6+ individual entity parameters
        // After: Only need to mock 1 container interface

        // Arrange
        var automation = new AirQualityAutomations(_mockEntities.Object, _mockLogger.Object);

        // Act & Assert - Easy to setup and verify interactions
        automation.Should().NotBeNull();

        // We can easily verify which entities the automation depends on
        _mockEntities.VerifyGet(x => x.Pm25Sensor, Times.AtLeastOnce);
        _mockEntities.VerifyGet(x => x.LedStatus, Times.AtLeastOnce);
    }

    [Fact]
    public void EntityContainer_Should_ProvideCorrectEntities()
    {
        // This test demonstrates that the entity container provides the correct entity types

        // Arrange & Act
        var automation = new AirQualityAutomations(_mockEntities.Object, _mockLogger.Object);

        // Assert - Verify that the container provides the expected entity types
        _mockEntities.VerifyGet(x => x.Pm25Sensor, Times.AtLeastOnce());
        _mockEntities.VerifyGet(x => x.LedStatus, Times.AtLeastOnce());

        // Verify the automation was created successfully
        automation.Should().NotBeNull();
    }
}
