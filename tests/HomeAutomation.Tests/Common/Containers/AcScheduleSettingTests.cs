using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.Tests.Common.Containers;

public class AcSettingsTests
{
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
    [InlineData(0, 23, true)]
    [InlineData(24, 5, false)]
    [InlineData(-1, 10, false)]
    public void IsValidHourRange_ReturnsCorrectResult(int hourStart, int hourEnd, bool expected)
    {
        var setting = new AcSettings(
            DoorOpenTemp: 26,
            EcoAwayTemp: 28,
            ComfortTemp: 23,
            AwayTemp: 29,
            Mode: "cool",
            ActivateFan: true,
            HourStart: hourStart,
            HourEnd: hourEnd
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
