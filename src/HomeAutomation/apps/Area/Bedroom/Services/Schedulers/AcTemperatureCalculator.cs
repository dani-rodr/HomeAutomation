using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

/// <summary>
/// Calculates appropriate AC temperature settings based on environmental conditions and user preferences.
/// </summary>
public class AcTemperatureCalculator(ILogger<AcTemperatureCalculator> logger)
    : IAcTemperatureCalculator
{
    private readonly ILogger<AcTemperatureCalculator> _logger = logger;

    public int CalculateTemperature(
        ClimateSetting settings,
        bool isOccupied,
        bool isDoorOpen,
        bool powerSaving,
        int powerSavingTempOffsetC
    )
    {
        var baseTemp = (isOccupied, isDoorOpen) switch
        {
            (true, false) => settings.ComfortTemp,
            (true, true) => settings.DoorOpenTemp,
            (false, _) => settings.AwayTemp,
        };

        var temp = powerSaving ? baseTemp + powerSavingTempOffsetC : baseTemp;

        _logger.LogDebug(
            "Temperature calculation: {Temperature}°C (base:{BaseTemperature}°C, offset:{Offset}°C) for conditions (occupied:{Occupied}, doorOpen:{DoorOpen}, powerSaving:{PowerSaving})",
            temp,
            baseTemp,
            powerSaving ? powerSavingTempOffsetC : 0,
            isOccupied,
            isDoorOpen,
            powerSaving
        );

        return temp;
    }
}
