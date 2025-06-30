using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Common.Containers;

public class AcSettingsTests
{
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
    [InlineData(0, 23, true)]
    [InlineData(24, 5, false)]
    [InlineData(-1, 10, false)]
    public void IsValidHourRange_ReturnsCorrectResult(int hourStart, int hourEnd, bool expected)
    {
        var setting = new AcSettings(
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

    [Fact]
    public void AcSettings_Record_PropertiesSetCorrectly()
    {
        var setting = CreateDefaultSetting();

        Assert.Equal(26, setting.NormalTemp);
        Assert.Equal(28, setting.PowerSavingTemp);
        Assert.Equal(23, setting.CoolTemp);
        Assert.Equal(29, setting.PassiveTemp);
        Assert.Equal("cool", setting.Mode);
        Assert.True(setting.ActivateFan);
        Assert.Equal(6, setting.HourStart);
        Assert.Equal(18, setting.HourEnd);
    }
}
