using System.ComponentModel.DataAnnotations;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.Kitchen.Config;

[AreaSettings("kitchen", "Kitchen", "Kitchen automation settings")]
public sealed class KitchenSettings
{
    [Required]
    public KitchenCookingSettings Cooking { get; init; } = new();

    [Required]
    public KitchenLightSettings Light { get; init; } = new();
}

public sealed class KitchenCookingSettings
{
    [Range(1, 180)]
    public int BoilingAutoOffMinutes { get; init; } = 12;

    [Range(500, 5000)]
    public int BoilingPowerThresholdWatts { get; init; } = 1550;
}

public sealed class KitchenLightSettings
{
    [Range(0, 120)]
    public int SensorWaitSeconds { get; init; } = 20;

    [Range(0, 120)]
    public int SensorActiveDelayValue { get; init; } = 20;

    [Range(0, 120)]
    public int SensorInactiveDelayValue { get; init; } = 3;

    [Range(0, 120)]
    public int MotionOnDelaySeconds { get; init; } = 1;
}
