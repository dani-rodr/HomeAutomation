using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.Tests.Common.Services;

public class AcTemperatureCalculatorTests
{
    private readonly Mock<ILogger<AcTemperatureCalculator>> _mockLogger;
    private readonly IAcTemperatureCalculator _calculator;
    private const int DefaultPowerSavingOffsetC = 2;

    public AcTemperatureCalculatorTests()
    {
        _mockLogger = new Mock<ILogger<AcTemperatureCalculator>>();

        _calculator = new AcTemperatureCalculator(_mockLogger.Object);
    }

    private static ClimateSetting CreateDefaultSetting() => new(26, 23, 25, "cool", true, 6, 18);

    [Theory]
    [InlineData(true, false, false, 23, "Occupied + door closed = ComfortTemp")]
    [InlineData(
        true,
        false,
        true,
        25,
        "Occupied + door closed + power saving = ComfortTemp + offset"
    )]
    [InlineData(true, true, false, 26, "Occupied + door open = DoorOpenTemp")]
    [InlineData(
        true,
        true,
        true,
        28,
        "Occupied + door open + power saving = DoorOpenTemp + offset"
    )]
    [InlineData(false, false, true, 27, "Unoccupied + power saving = AwayTemp + offset")]
    [InlineData(false, true, true, 27, "Unoccupied + door open + power saving = AwayTemp + offset")]
    [InlineData(false, false, false, 25, "Unoccupied + no power saving = AwayTemp")]
    [InlineData(false, true, false, 25, "Unoccupied + door open + no power saving = AwayTemp")]
    public void CalculateTemperature_ReturnsExpectedTemp(
        bool isOccupied,
        bool isDoorOpen,
        bool powerSaving,
        int expectedTemp,
        string _ // description, optional
    )
    {
        var setting = CreateDefaultSetting();
        var actualTemp = _calculator.CalculateTemperature(
            setting,
            isOccupied,
            isDoorOpen,
            powerSaving,
            DefaultPowerSavingOffsetC
        );

        Assert.Equal(expectedTemp, actualTemp);
    }

    [Fact]
    public void CalculateTemperature_LogsDebugInformation()
    {
        var setting = CreateDefaultSetting();
        _calculator.CalculateTemperature(setting, true, false, false, DefaultPowerSavingOffsetC);

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
