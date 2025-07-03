namespace HomeAutomation.apps.Common.Services;

/// <summary>
/// Calculates appropriate AC temperature settings based on environmental conditions and user preferences.
/// </summary>
public class AcTemperatureCalculator(
    IClimateSchedulerEntities entities,
    ILogger<AcTemperatureCalculator> logger
) : IAcTemperatureCalculator
{
    private readonly WeatherEntity _weather = entities.Weather;
    private readonly InputBooleanEntity _isPowerSavingMode = entities.PowerSavingMode;
    private readonly ILogger<AcTemperatureCalculator> _logger = logger;

    public int CalculateTemperature(AcSettings settings, bool isOccupied, bool isDoorOpen)
    {
        bool isColdWeather = !_weather.IsSunny();
        bool powerSaving = _isPowerSavingMode.IsOn();

        int temp;
        if (powerSaving)
        {
            temp = settings.PowerSavingTemp;
        }
        else if (isOccupied && !isDoorOpen)
        {
            temp = settings.CoolTemp;
        }
        else if (isDoorOpen && isColdWeather)
        {
            temp = settings.NormalTemp;
        }
        else if (isOccupied && isDoorOpen)
        {
            temp = settings.NormalTemp;
        }
        else
        {
            temp = settings.PassiveTemp;
        }
        _logger.LogDebug(
            "Temperature calculation: {Temperature}°C for conditions (occupied:{Occupied}, doorOpen:{DoorOpen}, powerSaving:{PowerSaving}, coldWeather:{ColdWeather})",
            temp,
            isOccupied,
            isDoorOpen,
            powerSaving,
            isColdWeather
        );

        return temp;
    }
}
