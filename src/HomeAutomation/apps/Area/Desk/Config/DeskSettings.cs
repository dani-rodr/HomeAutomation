using System.ComponentModel.DataAnnotations;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.Desk.Config;

[AreaSettings("desk", "Desk", "Desk automation settings")]
public sealed class DeskSettings
{
    [Required]
    public DeskLightSettings Light { get; init; } = new();
}

public sealed class DeskLightSettings : IValidatableObject
{
    [Display(
        Name = "Long Sensor Delay (s)",
        Description = "Long occupancy delay in seconds used for sustained desk presence."
    )]
    [Range(1, 600)]
    public int LongSensorDelaySeconds { get; init; } = 60;

    [Display(
        Name = "Short Sensor Delay (s)",
        Description = "Short occupancy delay in seconds used for quick desk transitions."
    )]
    [Range(1, 600)]
    public int ShortSensorDelaySeconds { get; init; } = 20;

    [Display(
        Name = "Brightness When Sala On",
        Description = "Desk light brightness level (1-255) when sala lights are on."
    )]
    [Range(1, 255)]
    public int BrightnessWhenSalaOn { get; init; } = 230;

    [Display(
        Name = "Brightness When Sala Off",
        Description = "Desk light brightness level (1-255) when sala lights are off."
    )]
    [Range(1, 255)]
    public int BrightnessWhenSalaOff { get; init; } = 125;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LongSensorDelaySeconds < ShortSensorDelaySeconds)
        {
            yield return new ValidationResult(
                "Long sensor delay should be greater than or equal to short sensor delay.",
                [nameof(LongSensorDelaySeconds)]
            );
        }
    }
}
