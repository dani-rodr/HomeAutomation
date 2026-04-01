namespace HomeAutomation.apps.Area.Bathroom.Config;

public sealed class BathroomSettings
{
    public required BathroomLightSettings Light { get; init; }
}

public sealed class BathroomLightSettings
{
    public int MotionOnDelaySeconds { get; init; } = 2;
    public int MasterSwitchDisableDelayMinutes { get; init; } = 5;
}
