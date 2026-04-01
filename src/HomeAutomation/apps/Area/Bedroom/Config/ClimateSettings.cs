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

    [Required]
    public BedroomLightSettings Light { get; init; } = new();

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
    [Range(1, 300)]
    public int SensorActiveDelayValue { get; init; } = 45;

    [Range(1, 10)]
    public int LightSwitchDoubleClickTimeoutSeconds { get; init; } = 2;
}

public sealed class ClimateAutomationSettings
{
    [Range(0, 24)]
    public int MasterSwitchReenableWhenNoMotionHours { get; init; } = 1;

    [Range(0, 48)]
    public int MasterSwitchReenableAfterOffHours { get; init; } = 8;

    [Range(0, 120)]
    public int DoorOpenReapplyMinutes { get; init; } = 5;

    [Range(0, 120)]
    public int MotionClearedReapplyMinutes { get; init; } = 10;

    [Range(0, 240)]
    public int HouseVacantTurnOffMinutes { get; init; } = 30;

    [Range(0, 240)]
    public int HouseReturnMinVacantMinutes { get; init; } = 20;

    [Required]
    public string ResetScheduleCron { get; init; } = "0 0 * * *";
}

public sealed class WeatherPowerSavingSettings : IValidatableObject
{
    [Range(0, 20)]
    public double TriggerUvIndex { get; init; }

    [Range(10, 45)]
    public double TriggerOutdoorTempC { get; init; }

    [Range(0, 20)]
    public double RecoveryUvIndex { get; init; }

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

    [Range(0, 23)]
    public int HourStart { get; init; }

    [Range(0, 23)]
    public int HourEnd { get; init; }

    [Range(16, 30)]
    public int DoorOpenTemp { get; init; }

    [Range(16, 30)]
    public int EcoAwayTemp { get; init; }

    [Range(16, 30)]
    public int ComfortTemp { get; init; }

    [Range(16, 30)]
    public int AwayTemp { get; init; }

    [Required]
    public string Mode { get; init; } = HaEntityStates.COOL;

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
