using System.ComponentModel.DataAnnotations;
using System.Linq;
using HomeAutomation.apps.Common.Settings;

namespace HomeAutomation.apps.Area.Bedroom.Config;

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

[AreaSettings("bedroom", "Bedroom", "Bedroom climate automation settings")]
public sealed class BedroomSettings
{
    [Required]
    public ClimateSettings Climate { get; init; } = new();

    [Required]
    public BedroomLightSettings Light { get; init; } = new();
}

public sealed class ClimateSettings
{
    [Required]
    public ClimateSetting Sunrise { get; init; } = new();

    [Required]
    public ClimateSetting Sunset { get; init; } = new();

    [Required]
    public ClimateSetting Midnight { get; init; } = new();

    [Required]
    public WeatherPowerSavingSettings WeatherPowerSaving { get; init; } = new();

    [Required]
    public ClimateAutomationSettings Automation { get; init; } = new();

    public ClimateSetting GetByTimeBlock(TimeBlock timeBlock) =>
        timeBlock switch
        {
            TimeBlock.Sunrise => Sunrise,
            TimeBlock.Sunset => Sunset,
            TimeBlock.Midnight => Midnight,
            _ => throw new ArgumentOutOfRangeException(nameof(timeBlock), timeBlock, null),
        };
}

public sealed class BedroomLightSettings
{
    [Display(
        Name = "Sensor Active Delay (s)",
        Description = "Delay in seconds before bedroom occupancy is considered active for lighting."
    )]
    [Range(1, 300)]
    public int SensorActiveDelayValue { get; init; } = 45;

    [Display(
        Name = "Light Switch Double Click Timeout (s)",
        Description = "Maximum gap in seconds between button presses to detect a double click."
    )]
    [Range(1, 10)]
    public int LightSwitchDoubleClickTimeoutSeconds { get; init; } = 2;
}

public sealed class ClimateAutomationSettings
{
    [Display(
        Name = "Reenable Without Motion (h)",
        Description = "Hours without motion before re-enabling the bedroom climate master switch."
    )]
    [Range(0, 24)]
    public int MasterSwitchReenableWhenNoMotionHours { get; init; } = 1;

    [Display(
        Name = "Reenable After Manual Off (h)",
        Description = "Hours after manual turn-off before climate automation can re-enable."
    )]
    [Range(0, 48)]
    public int MasterSwitchReenableAfterOffHours { get; init; } = 8;

    [Display(
        Name = "Door Open Reapply (min)",
        Description = "Minutes after door-open events before climate settings are re-applied."
    )]
    [Range(0, 120)]
    public int DoorOpenReapplyMinutes { get; init; } = 5;

    [Display(
        Name = "Motion Cleared Reapply (min)",
        Description = "Minutes after motion clears before climate settings are re-applied."
    )]
    [Range(0, 120)]
    public int MotionClearedReapplyMinutes { get; init; } = 10;

    [Display(
        Name = "Vacant Turn-Off (min)",
        Description = "Minutes the house must be vacant before turning bedroom AC off."
    )]
    [Range(0, 240)]
    public int HouseVacantTurnOffMinutes { get; init; } = 30;

    [Display(
        Name = "Return Minimum Vacant (min)",
        Description = "Minimum vacant minutes required before return-home climate logic runs."
    )]
    [Range(0, 240)]
    public int HouseReturnMinVacantMinutes { get; init; } = 20;

    [Display(
        Name = "Reset Schedule Cron",
        Description = "Cron expression for scheduled bedroom climate reset checks."
    )]
    [Required]
    public string ResetScheduleCron { get; init; } = "0 0 * * *";
}

public sealed class WeatherPowerSavingSettings : IValidatableObject
{
    [Display(
        Name = "Trigger UV Index",
        Description = "UV index threshold that enables weather-based power saving."
    )]
    [Range(0, 20)]
    public double TriggerUvIndex { get; init; }

    [Display(
        Name = "Trigger Outdoor Temp (C)",
        Description = "Outdoor temperature threshold in Celsius that enables weather-based power saving."
    )]
    [Range(10, 45)]
    public double TriggerOutdoorTempC { get; init; }

    [Display(
        Name = "Recovery UV Index",
        Description = "UV index threshold below which weather-based power saving can recover."
    )]
    [Range(0, 20)]
    public double RecoveryUvIndex { get; init; }

    [Display(
        Name = "Recovery Outdoor Temp (C)",
        Description = "Outdoor temperature threshold in Celsius below which weather-based power saving can recover."
    )]
    [Range(10, 45)]
    public double RecoveryOutdoorTempC { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TriggerUvIndex < RecoveryUvIndex)
        {
            yield return new ValidationResult(
                "TriggerUvIndex must be greater than or equal to RecoveryUvIndex.",
                [nameof(TriggerUvIndex)]
            );
        }

        if (TriggerOutdoorTempC < RecoveryOutdoorTempC)
        {
            yield return new ValidationResult(
                "TriggerOutdoorTempC must be greater than or equal to RecoveryOutdoorTempC.",
                [nameof(TriggerOutdoorTempC)]
            );
        }
    }
}

public sealed class ClimateSetting : IValidatableObject
{
    public ClimateSetting() { }

    public ClimateSetting(
        int doorOpenTemp,
        int ecoAwayTemp,
        int comfortTemp,
        int awayTemp,
        string mode,
        bool activateFan,
        int hourStart,
        int hourEnd
    )
    {
        DoorOpenTemp = doorOpenTemp;
        EcoAwayTemp = ecoAwayTemp;
        ComfortTemp = comfortTemp;
        AwayTemp = awayTemp;
        Mode = mode;
        ActivateFan = activateFan;
        HourStart = hourStart;
        HourEnd = hourEnd;
    }

    [Display(
        Name = "Hour Start",
        Description = "Starting hour (0-23) when this climate block becomes active."
    )]
    [Range(0, 23)]
    public int HourStart { get; init; }

    [Display(
        Name = "Hour End",
        Description = "Ending hour (0-23) when this climate block stops being active."
    )]
    [Range(0, 23)]
    public int HourEnd { get; init; }

    [Display(
        Name = "Door Open Temp",
        Description = "Target temperature used when the bedroom door is open."
    )]
    [Range(16, 30)]
    public int DoorOpenTemp { get; init; }

    [Display(
        Name = "Eco Away Temp",
        Description = "Energy-saving away temperature for this climate block."
    )]
    [Range(16, 30)]
    public int EcoAwayTemp { get; init; }

    [Display(
        Name = "Comfort Temp",
        Description = "Comfort temperature used while room conditions are favorable."
    )]
    [Range(16, 30)]
    public int ComfortTemp { get; init; }

    [Display(Name = "Away Temp", Description = "Regular away temperature for this climate block.")]
    [Range(16, 30)]
    public int AwayTemp { get; init; }

    [Display(Name = "Mode", Description = "Climate mode to apply (cool, dry, auto, fan_only).")]
    [Required]
    public string Mode { get; init; } = HaEntityStates.COOL;

    [Display(
        Name = "Activate Fan",
        Description = "Whether fan mode enhancements should be enabled for this climate block."
    )]
    public bool ActivateFan { get; init; }

    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (
            !new[]
            {
                HaEntityStates.COOL,
                HaEntityStates.DRY,
                HaEntityStates.AUTO,
                "fan_only",
            }.Contains(Mode, StringComparer.OrdinalIgnoreCase)
        )
        {
            yield return new ValidationResult(
                "Mode must be one of: cool, dry, auto, fan_only.",
                [nameof(Mode)]
            );
        }

        if (ComfortTemp > DoorOpenTemp)
        {
            yield return new ValidationResult(
                "ComfortTemp must be less than or equal to DoorOpenTemp.",
                [nameof(ComfortTemp)]
            );
        }
    }
}
