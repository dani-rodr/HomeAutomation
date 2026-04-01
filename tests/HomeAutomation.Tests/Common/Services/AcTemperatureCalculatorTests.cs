using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers.Entities;

namespace HomeAutomation.Tests.Common.Services;

public class AcTemperatureCalculatorTests : HaContextTestBase
{
    private MockHaContext _mockHaContext => HaContext;
    private readonly InputBooleanEntity _powerSaving;
    private readonly Mock<ILogger<AcTemperatureCalculator>> _mockLogger;
    private readonly Mock<IClimateSchedulerEntities> _mockEntities;
    private readonly IAcTemperatureCalculator _calculator;

    public AcTemperatureCalculatorTests()
    {
        _powerSaving = new InputBooleanEntity(_mockHaContext, "input_boolean.power_saving_mode");
        _mockLogger = new Mock<ILogger<AcTemperatureCalculator>>();
        _mockEntities = new Mock<IClimateSchedulerEntities>();

        _mockEntities.Setup(e => e.PowerSavingMode).Returns(_powerSaving);

        _calculator = new AcTemperatureCalculator(_mockEntities.Object, _mockLogger.Object);
    }

    private static AcSettings CreateDefaultSetting() =>
        new(
            DoorOpenTemp: 26,
            EcoAwayTemp: 28,
            ComfortTemp: 23,
            AwayTemp: 29,
            Mode: "cool",
            ActivateFan: true,
            HourStart: 6,
            HourEnd: 18
        );

    [Theory]
    [InlineData(true, false, "off", 23, "Occupied + door closed = ComfortTemp")]
    [InlineData(true, false, "on", 23, "Occupied + door closed ignores power saving = ComfortTemp")]
    [InlineData(true, true, "off", 26, "Occupied + door open = DoorOpenTemp")]
    [InlineData(true, true, "on", 26, "Occupied + door open ignores power saving = DoorOpenTemp")]
    [InlineData(false, false, "on", 28, "Unoccupied + power saving = EcoAwayTemp")]
    [InlineData(false, true, "on", 28, "Unoccupied + door open + power saving = EcoAwayTemp")]
    [InlineData(false, false, "off", 29, "Unoccupied + no power saving = AwayTemp")]
    [InlineData(false, true, "off", 29, "Unoccupied + door open + no power saving = AwayTemp")]
    public void CalculateTemperature_ReturnsExpectedTemp(
        bool isOccupied,
        bool isDoorOpen,
        string powerSavingState,
        int expectedTemp,
        string _ // description, optional
    )
    {
        _mockHaContext.SetEntityState(_powerSaving.EntityId, powerSavingState);

        var setting = CreateDefaultSetting();
        var actualTemp = _calculator.CalculateTemperature(setting, isOccupied, isDoorOpen);

        Assert.Equal(expectedTemp, actualTemp);
    }

    [Fact]
    public void CalculateTemperature_LogsDebugInformation()
    {
        _mockHaContext.SetEntityState(_powerSaving.EntityId, "off");

        var setting = CreateDefaultSetting();
        _calculator.CalculateTemperature(setting, true, false);

        _mockLogger.Verify(
            l =>
                l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Temperature calculation")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
