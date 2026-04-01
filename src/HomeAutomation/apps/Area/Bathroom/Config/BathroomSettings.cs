using System.ComponentModel.DataAnnotations;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.Bathroom.Config;

[AreaSettings("bathroom", "Bathroom", "Bathroom automation settings")]
public sealed class BathroomSettings
{
    [Required]
    public BathroomLightSettings Light { get; init; } = new();
}

public sealed class BathroomLightSettings
{
    [Range(0, 30)]
    public int MotionOnDelaySeconds { get; init; } = 2;

    [Range(1, 60)]
    public int MasterSwitchDisableDelayMinutes { get; init; } = 5;
}
