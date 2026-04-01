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
    [Range(0, 100)]
    public int CleanThresholdPm25 { get; init; } = 7;

    [Range(1, 500)]
    public int DirtyThresholdPm25 { get; init; } = 75;

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
    [Range(1, 300)]
    public int SensorWaitSeconds { get; init; } = 30;

    [Range(1, 900)]
    public int SensorActiveDelayValue { get; init; } = 45;

    [Range(1, 60)]
    public int SensorInactiveDelayValue { get; init; } = 1;

    [Range(1, 100)]
    public int DimmingBrightnessPct { get; init; } = 80;

    [Range(1, 300)]
    public int DimmingDelaySeconds { get; init; } = 15;

    [Range(1, 240)]
    public int TvOffMasterSwitchReenableMinutes { get; init; } = 30;

    [Range(1, 120)]
    public int KitchenOccupancyDelaySeconds { get; init; } = 10;
}

public sealed class LivingRoomFanSettings
{
    [Range(1, 120)]
    public int MotionOnDelaySeconds { get; init; } = 3;

    [Range(1, 60)]
    public int MotionOffDelayMinutes { get; init; } = 1;
}
