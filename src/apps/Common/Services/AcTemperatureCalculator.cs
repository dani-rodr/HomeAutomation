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

        var temp = (isOccupied, isDoorOpen, powerSaving, isColdWeather) switch
        {
            (_, _, true, _) => settings.PowerSavingTemp,
            (true, false, _, _) => settings.CoolTemp,
            (_, true, _, true) => settings.NormalTemp,
            (true, true, _, false) => settings.NormalTemp,
            (false, true, _, false) => settings.PassiveTemp,
            (false, false, _, _) => settings.PassiveTemp,
        };

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
