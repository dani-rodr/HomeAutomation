using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.Tests.Common.Services;

public class AcTemperatureCalculatorTests
{
    private readonly Mock<ILogger<AcTemperatureCalculator>> _mockLogger;
    private readonly IAcTemperatureCalculator _calculator;

    public AcTemperatureCalculatorTests()
    {
        _mockLogger = new Mock<ILogger<AcTemperatureCalculator>>();

        _calculator = new AcTemperatureCalculator(_mockLogger.Object);
    }

    private static ClimateSetting CreateDefaultSetting() =>
        new(26, 28, 23, 29, "cool", true, 6, 18);

    [Theory]
    [InlineData(true, false, false, 23, "Occupied + door closed = ComfortTemp")]
    [InlineData(true, false, true, 23, "Occupied + door closed ignores power saving = ComfortTemp")]
    [InlineData(true, true, false, 26, "Occupied + door open = DoorOpenTemp")]
    [InlineData(true, true, true, 26, "Occupied + door open ignores power saving = DoorOpenTemp")]
    [InlineData(false, false, true, 28, "Unoccupied + power saving = EcoAwayTemp")]
    [InlineData(false, true, true, 28, "Unoccupied + door open + power saving = EcoAwayTemp")]
    [InlineData(false, false, false, 29, "Unoccupied + no power saving = AwayTemp")]
    [InlineData(false, true, false, 29, "Unoccupied + door open + no power saving = AwayTemp")]
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
            powerSaving
        );

        Assert.Equal(expectedTemp, actualTemp);
    }

    [Fact]
    public void CalculateTemperature_LogsDebugInformation()
    {
        var setting = CreateDefaultSetting();
        _calculator.CalculateTemperature(setting, true, false, false);

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
