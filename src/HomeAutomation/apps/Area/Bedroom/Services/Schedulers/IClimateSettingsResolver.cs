using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

public interface IClimateSettingsResolver : IAutomationScheduler
{
    IDisposable GetResetSchedule();
    bool TryGetCurrentSetting(out TimeBlock timeBlock, out ClimateSetting setting);
    WeatherPowerSavingSettings GetWeatherPowerSavingSettings();
    ClimateAutomationSettings GetAutomationSettings();
    int CalculateTemperature(ClimateSetting settings, bool isOccupied, bool isDoorOpen);
}
