namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

public record AcSettings(
    int DoorOpenTemp,
    int EcoAwayTemp,
    int ComfortTemp,
    int AwayTemp,
    string Mode,
    bool ActivateFan,
    int HourStart,
    int HourEnd
)
{
    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;
}
