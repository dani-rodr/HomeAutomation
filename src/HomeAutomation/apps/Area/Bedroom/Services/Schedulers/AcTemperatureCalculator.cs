namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

/// <summary>
/// Calculates appropriate AC temperature settings based on environmental conditions and user preferences.
/// </summary>
public class AcTemperatureCalculator(ILogger<AcTemperatureCalculator> logger)
    : IAcTemperatureCalculator
{
    private readonly ILogger<AcTemperatureCalculator> _logger = logger;

    public int CalculateTemperature(
        AcSettings settings,
        bool isOccupied,
        bool isDoorOpen,
        bool powerSaving
    )
    {
        int temp;
        if (isOccupied && !isDoorOpen)
        {
            temp = settings.ComfortTemp;
        }
        else if (isOccupied)
        {
            temp = settings.DoorOpenTemp;
        }
        else if (powerSaving)
        {
            temp = settings.EcoAwayTemp;
        }
        else
        {
            temp = settings.AwayTemp;
        }

        _logger.LogDebug(
            "Temperature calculation: {Temperature}°C for conditions (occupied:{Occupied}, doorOpen:{DoorOpen}, powerSaving:{PowerSaving})",
            temp,
            isOccupied,
            isDoorOpen,
            powerSaving
        );

        return temp;
    }
}
