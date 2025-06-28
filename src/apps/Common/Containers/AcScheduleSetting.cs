namespace HomeAutomation.apps.Common.Containers;

public enum TimeBlock
{
    Sunrise,
    Sunset,
    Midnight,
}

public class AcScheduleSetting(
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
    public int NormalTemp { get; } = NormalTemp;
    public int PowerSavingTemp { get; } = PowerSavingTemp;
    public int CoolTemp { get; } = CoolTemp;
    public int PassiveTemp { get; } = PassiveTemp;
    public string Mode { get; } = Mode;
    public bool ActivateFan { get; } = ActivateFan;
    public int HourStart { get; } = HourStart;
    public int HourEnd { get; } = HourEnd;
    private static WeatherEntity? _weather;
    private static InputBooleanEntity? _isPowerSavingMode;
    private static ILogger? _logger;

    public static void Initialize(
        WeatherEntity weather,
        InputBooleanEntity isPowerSavingMode,
        ILogger logger
    )
    {
        _weather = weather;
        _logger = logger;
        _isPowerSavingMode = isPowerSavingMode;
    }

    public bool IsValidHourRange() => HourStart is >= 0 and <= 23 && HourEnd is >= 0 and <= 23;

    public int GetTemperature(bool isOccupied, bool isDoorOpen)
    {
        bool isCold = _weather != null && !_weather.IsSunny();
        var temp = (isOccupied, isDoorOpen, _isPowerSavingMode.IsOn(), isCold) switch
        {
            (_, _, true, _) => PowerSavingTemp,
            (true, false, _, _) => CoolTemp,
            (_, true, _, true) => NormalTemp,
            (true, true, _, false) => NormalTemp,
            (false, true, _, false) => PassiveTemp,
            (false, false, _, _) => PassiveTemp,
        };

        _logger?.LogDebug(
            "Temperature decision: Selected temperature {Temperature}°C based on pattern: (occupied:{Occupied}, doorOpen:{DoorOpen}, powerSaving:{PowerSaving})",
            temp,
            isOccupied,
            isDoorOpen,
            _isPowerSavingMode.IsOn()
        );
        return temp;
    }
}
