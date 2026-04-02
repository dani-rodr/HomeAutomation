using System.ComponentModel.DataAnnotations;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.LivingRoom.Config;

[AreaSettings("livingroom", "Living Room", "Living room automation settings")]
public sealed class LivingRoomSettings
{
    [Required]
    public LivingRoomAirQualitySettings AirQuality { get; init; } = new();

    [Required]
    public LivingRoomLightSettings Light { get; init; } = new();

    [Required]
    public LivingRoomFanSettings Fan { get; init; } = new();
}

public sealed class LivingRoomAirQualitySettings : IValidatableObject
{
    [Display(
        Name = "Clean Threshold PM2.5",
        Description = "PM2.5 value at or below which air quality is treated as clean."
    )]
    [Range(0, 100)]
    public int CleanThresholdPm25 { get; init; } = 7;

    [Display(
        Name = "Dirty Threshold PM2.5",
        Description = "PM2.5 value at or above which air quality is treated as dirty."
    )]
    [Range(1, 500)]
    public int DirtyThresholdPm25 { get; init; } = 75;

    [Display(
        Name = "Manual Override Reset (min)",
        Description = "Minutes before manual fan override is cleared."
    )]
    [Range(1, 180)]
    public int ManualOverrideResetMinutes { get; init; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DirtyThresholdPm25 < CleanThresholdPm25)
        {
            yield return new ValidationResult(
                "Dirty threshold must be greater than or equal to clean threshold.",
                [nameof(DirtyThresholdPm25)]
            );
        }
    }
}

public sealed class LivingRoomLightSettings
{
    [Display(
        Name = "Sensor Wait (s)",
        Description = "Base wait time for living room occupancy sensor transitions."
    )]
    [Range(1, 300)]
    public int SensorWaitSeconds { get; init; } = 30;

    [Display(
        Name = "Sensor Active Delay (s)",
        Description = "Delay in seconds before occupancy is considered active."
    )]
    [Range(1, 900)]
    public int SensorActiveDelayValue { get; init; } = 45;

    [Display(
        Name = "Sensor Inactive Delay (s)",
        Description = "Delay in seconds before occupancy is considered inactive."
    )]
    [Range(1, 60)]
    public int SensorInactiveDelayValue { get; init; } = 1;

    [Display(
        Name = "Dimming Brightness (%)",
        Description = "Brightness percentage used during dimming transitions."
    )]
    [Range(1, 100)]
    public int DimmingBrightnessPct { get; init; } = 80;

    [Display(
        Name = "Dimming Delay (s)",
        Description = "Seconds to wait before dimming lights after trigger."
    )]
    [Range(1, 300)]
    public int DimmingDelaySeconds { get; init; } = 15;

    [Display(
        Name = "TV-Off Reenable (min)",
        Description = "Minutes after TV turns off before re-enabling master switch automation."
    )]
    [Range(1, 240)]
    public int TvOffMasterSwitchReenableMinutes { get; init; } = 30;

    [Display(
        Name = "Kitchen Occupancy Delay (s)",
        Description = "Delay in seconds used when kitchen occupancy influences living room lights."
    )]
    [Range(1, 120)]
    public int KitchenOccupancyDelaySeconds { get; init; } = 10;
}

public sealed class LivingRoomFanSettings
{
    [Display(
        Name = "Motion On Delay (s)",
        Description = "Seconds to wait after motion before turning the living room fan on."
    )]
    [Range(1, 120)]
    public int MotionOnDelaySeconds { get; init; } = 3;

    [Display(
        Name = "Motion Off Delay (min)",
        Description = "Minutes to wait after motion clears before turning the living room fan off."
    )]
    [Range(1, 60)]
    public int MotionOffDelayMinutes { get; init; } = 1;
}
