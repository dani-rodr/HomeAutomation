using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Common.Containers;

public class AcScheduleSettingTests
{
    private readonly MockHaContext _mockHaContext;
    private readonly WeatherEntity _weather;
    private readonly InputBooleanEntity _powerSaving;
    private readonly Mock<ILogger> _mockLogger;

    public AcScheduleSettingTests()
    {
        _mockHaContext = new MockHaContext();
        _weather = new WeatherEntity(_mockHaContext, "weather.home");
        _powerSaving = new InputBooleanEntity(_mockHaContext, "input_boolean.power_saving_mode");
        _mockLogger = new Mock<ILogger>();

        AcScheduleSetting.Initialize(_weather, _powerSaving, _mockLogger.Object);
    }

    private static AcScheduleSetting CreateDefaultSetting() =>
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
    public void GetTemperature_ReturnsExpectedTemp(
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
        var actualTemp = setting.GetTemperature(isOccupied, isDoorOpen);

        Assert.Equal(expectedTemp, actualTemp);
    }

    [Theory]
    [InlineData(0, 23, true)]
    [InlineData(24, 5, false)]
    [InlineData(-1, 10, false)]
    public void IsValidHourRange_ReturnsCorrectResult(int hourStart, int hourEnd, bool expected)
    {
        var setting = new AcScheduleSetting(
            NormalTemp: 26,
            PowerSavingTemp: 28,
            CoolTemp: 23,
            PassiveTemp: 29,
            Mode: "cool",
            ActivateFan: true,
            HourStart: hourStart,
            HourEnd: hourEnd
        );

        Assert.Equal(expected, setting.IsValidHourRange());
    }
}
