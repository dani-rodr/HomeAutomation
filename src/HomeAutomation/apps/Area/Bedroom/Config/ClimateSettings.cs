namespace HomeAutomation.apps.Area.Bedroom.Config;

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

public sealed class ClimateSettings
{
    public required ClimateSetting Sunrise { get; init; }
    public required ClimateSetting Sunset { get; init; }
    public required ClimateSetting Midnight { get; init; }
    public required WeatherPowerSavingSettings WeatherPowerSaving { get; init; }

    public ClimateSetting GetByTimeBlock(TimeBlock timeBlock) =>
        timeBlock switch
        {
            TimeBlock.Sunrise => Sunrise,
            TimeBlock.Sunset => Sunset,
            TimeBlock.Midnight => Midnight,
            _ => throw new ArgumentOutOfRangeException(nameof(timeBlock), timeBlock, null),
        };
}

public sealed class WeatherPowerSavingSettings
{
    public double TriggerUvIndex { get; init; }
    public double TriggerOutdoorTempC { get; init; }
    public double RecoveryUvIndex { get; init; }
    public double RecoveryOutdoorTempC { get; init; }
}

public sealed class ClimateSetting
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

    public int HourStart { get; init; }
    public int HourEnd { get; init; }
    public int DoorOpenTemp { get; init; }
    public int EcoAwayTemp { get; init; }
    public int ComfortTemp { get; init; }
    public int AwayTemp { get; init; }
    public string Mode { get; init; } = HaEntityStates.COOL;
    public bool ActivateFan { get; init; }

    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;
}
