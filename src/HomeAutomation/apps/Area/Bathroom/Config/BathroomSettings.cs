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
    [Display(
        Name = "Motion On Delay (s)",
        Description = "Seconds to wait after motion is detected before turning bathroom lights on."
    )]
    [Range(0, 30)]
    public int MotionOnDelaySeconds { get; init; } = 2;

    [Display(
        Name = "Master Switch Disable Delay (min)",
        Description = "Minutes to keep automations disabled after the bathroom master switch is turned off."
    )]
    [Range(1, 60)]
    public int MasterSwitchDisableDelayMinutes { get; init; } = 5;
}
