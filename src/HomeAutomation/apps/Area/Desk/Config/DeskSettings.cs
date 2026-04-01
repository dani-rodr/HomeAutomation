namespace HomeAutomation.apps.Area.Desk.Config;

public sealed class DeskSettings
{
    public required DeskLightSettings Light { get; init; }
}

public sealed class DeskLightSettings
{
    public int LongSensorDelaySeconds { get; init; } = 60;
    public int ShortSensorDelaySeconds { get; init; } = 20;
    public int BrightnessWhenSalaOn { get; init; } = 230;
    public int BrightnessWhenSalaOff { get; init; } = 125;
}
