namespace HomeAutomation.apps.Common.Containers;

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

public record AcSettings(
    int NormalTemp,
    int PowerSavingTemp,
    int CoolTemp,
    int PassiveTemp,
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
)
{
    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;
}
