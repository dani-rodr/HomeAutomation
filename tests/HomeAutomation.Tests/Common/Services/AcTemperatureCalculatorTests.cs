using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;
using HomeAutomation.apps.Common.Services;

namespace HomeAutomation.Tests.Common.Services;

public class AcTemperatureCalculatorTests
{
    private readonly MockHaContext _mockHaContext;
    private readonly WeatherEntity _weather;
    private readonly InputBooleanEntity _powerSaving;
    private readonly Mock<ILogger<AcTemperatureCalculator>> _mockLogger;
    private readonly Mock<IClimateSchedulerEntities> _mockEntities;
    private readonly IAcTemperatureCalculator _calculator;

    public AcTemperatureCalculatorTests()
    {
        _mockHaContext = new MockHaContext();
        _weather = new WeatherEntity(_mockHaContext, "weather.home");
        _powerSaving = new InputBooleanEntity(_mockHaContext, "input_boolean.power_saving_mode");
        _mockLogger = new Mock<ILogger<AcTemperatureCalculator>>();
        _mockEntities = new Mock<IClimateSchedulerEntities>();

        _mockEntities.Setup(e => e.Weather).Returns(_weather);
        _mockEntities.Setup(e => e.PowerSavingMode).Returns(_powerSaving);

        _calculator = new AcTemperatureCalculator(_mockEntities.Object, _mockLogger.Object);
    }

    private static AcSettings CreateDefaultSetting() =>
        new(
            NormalTemp: 26,
            PowerSavingTemp: 28,
            CoolTemp: 23,
            PassiveTemp: 29,
            Mode: "cool",
            ActivateFan: true,
            HourStart: 6,
            HourEnd: 18
        );

    [Theory]
    [InlineData(
        true,
        false,
        "off",
        "cloudy",
        23,
        "Occupied + door closed, not saving, cold = CoolTemp"
    )]
    [InlineData(false, true, "off", "sunny", 29, "Unoccupied + door open + not cold = PassiveTemp")]
    [InlineData(true, true, "off", "sunny", 26, "Occupied + door open + not cold = NormalTemp")]
    [InlineData(false, false, "on", "sunny", 28, "Power saving active overrides = PowerSavingTemp")]
    [InlineData(true, true, "off", "cloudy", 26, "Occupied + door open + cold = NormalTemp")]
    [InlineData(false, false, "off", "sunny", 29, "Unoccupied + door closed = PassiveTemp")]
    public void CalculateTemperature_ReturnsExpectedTemp(
        bool isOccupied,
        bool isDoorOpen,
        string powerSavingState,
        string weatherState,
        int expectedTemp,
        string _ // description, optional
    )
    {
        _mockHaContext.SetEntityState(_powerSaving.EntityId, powerSavingState);
        _mockHaContext.SetEntityState(_weather.EntityId, weatherState);

        var setting = CreateDefaultSetting();
        var actualTemp = _calculator.CalculateTemperature(setting, isOccupied, isDoorOpen);

        Assert.Equal(expectedTemp, actualTemp);
    }

    [Fact]
    public void CalculateTemperature_LogsDebugInformation()
    {
        _mockHaContext.SetEntityState(_powerSaving.EntityId, "off");
        _mockHaContext.SetEntityState(_weather.EntityId, "sunny");

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
