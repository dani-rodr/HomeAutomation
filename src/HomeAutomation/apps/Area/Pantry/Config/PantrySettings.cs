using System.ComponentModel.DataAnnotations;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.Pantry.Config;

[AreaSettings("pantry", "Pantry", "Pantry automation settings")]
public sealed class PantrySettings
{
    [Required]
    public PantryLightSettings Light { get; init; } = new();
}

public sealed class PantryLightSettings
{
    [Display(Name = "Sensor Wait (s)", Description = "Base pantry sensor wait time in seconds.")]
    [Range(1, 120)]
    public int SensorWaitSeconds { get; init; } = 5;

    [Display(
        Name = "Sensor Active Delay (s)",
        Description = "Delay in seconds before pantry occupancy is considered active."
    )]
    [Range(1, 120)]
    public int SensorActiveDelayValue { get; init; } = 5;

    [Display(
        Name = "Bathroom Turn-Off Delay (s)",
        Description = "Delay in seconds before bathroom automation is turned off after pantry inactivity."
    )]
    [Range(10, 600)]
    public int BathroomAutomationTurnOffDelaySeconds { get; init; } = 60;
}
