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
    [Display(
        Name = "Boiling Auto-Off (min)",
        Description = "Maximum minutes the boiler can stay active before auto shut-off."
    )]
    [Range(1, 180)]
    public int BoilingAutoOffMinutes { get; init; } = 12;

    [Display(
        Name = "Boiling Power Threshold (W)",
        Description = "Power level in watts that indicates boiling mode is active."
    )]
    [Range(500, 5000)]
    public int BoilingPowerThresholdWatts { get; init; } = 1550;
}

public sealed class KitchenLightSettings
{
    [Display(
        Name = "Sensor Wait (s)",
        Description = "Base wait time used by the kitchen sensor logic before state transitions."
    )]
    [Range(0, 120)]
    public int SensorWaitSeconds { get; init; } = 20;

    [Display(
        Name = "Sensor Active Delay (s)",
        Description = "Delay in seconds before treating the kitchen sensor as actively occupied."
    )]
    [Range(0, 120)]
    public int SensorActiveDelayValue { get; init; } = 20;

    [Display(
        Name = "Sensor Inactive Delay (s)",
        Description = "Delay in seconds before treating the kitchen sensor as inactive."
    )]
    [Range(0, 120)]
    public int SensorInactiveDelayValue { get; init; } = 3;

    [Display(
        Name = "Motion On Delay (s)",
        Description = "Seconds to wait after kitchen motion before turning lights on."
    )]
    [Range(0, 120)]
    public int MotionOnDelaySeconds { get; init; } = 1;
}
