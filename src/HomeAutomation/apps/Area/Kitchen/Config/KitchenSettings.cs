namespace HomeAutomation.apps.Area.Kitchen.Config;

public sealed class KitchenSettings
{
    public required KitchenCookingSettings Cooking { get; init; }
    public required KitchenLightSettings Light { get; init; }
}

public sealed class KitchenCookingSettings
{
    public int BoilingAutoOffMinutes { get; init; } = 12;
    public int BoilingPowerThresholdWatts { get; init; } = 1550;
}

public sealed class KitchenLightSettings
{
    public int SensorWaitSeconds { get; init; } = 20;
    public int SensorActiveDelayValue { get; init; } = 20;
    public int SensorInactiveDelayValue { get; init; } = 3;
    public int MotionOnDelaySeconds { get; init; } = 1;
}
