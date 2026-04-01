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
    [Range(1, 120)]
    public int SensorWaitSeconds { get; init; } = 5;

    [Range(1, 120)]
    public int SensorActiveDelayValue { get; init; } = 5;

    [Range(10, 600)]
    public int BathroomAutomationTurnOffDelaySeconds { get; init; } = 60;
}
