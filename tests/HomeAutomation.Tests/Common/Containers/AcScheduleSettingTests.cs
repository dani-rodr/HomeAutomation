using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.Tests.Common.Containers;

public class AcSettingsTests
{
    private static ClimateSetting CreateDefaultSetting() =>
        new(26, 28, 23, 29, "cool", true, 6, 18);

    [Theory]
    [InlineData(0, 23, true)]
    [InlineData(24, 5, false)]
    [InlineData(-1, 10, false)]
    public void IsValidHourRange_ReturnsCorrectResult(int hourStart, int hourEnd, bool expected)
    {
        var setting = new ClimateSetting(
            doorOpenTemp: 26,
            ecoAwayTemp: 28,
            comfortTemp: 23,
            awayTemp: 29,
            mode: "cool",
            activateFan: true,
            hourStart: hourStart,
            hourEnd: hourEnd
        );

        Assert.Equal(expected, setting.IsValidHourRange());
    }

    [Fact]
    public void AcSettings_Record_PropertiesSetCorrectly()
    {
        var setting = CreateDefaultSetting();

        Assert.Equal(26, setting.DoorOpenTemp);
        Assert.Equal(28, setting.EcoAwayTemp);
        Assert.Equal(23, setting.ComfortTemp);
        Assert.Equal(29, setting.AwayTemp);
        Assert.Equal("cool", setting.Mode);
        Assert.True(setting.ActivateFan);
        Assert.Equal(6, setting.HourStart);
        Assert.Equal(18, setting.HourEnd);
    }
}
