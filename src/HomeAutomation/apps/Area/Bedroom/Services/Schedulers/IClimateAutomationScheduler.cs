using HomeAutomation.apps.Area.Bedroom.Config;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

public interface IClimateAutomationScheduler : IAutomationScheduler
{
    IObservable<ClimateSettings> Changes { get; }

    IDisposable GetResetSchedule();
    bool TryGetCurrentSetting(out TimeBlock timeBlock, out ClimateSetting setting);
    ClimateSettings GetCurrentSettings();
    int CalculateTemperature(
        ClimateSetting settings,
        bool isOccupied,
        bool isDoorOpen,
        bool applyPowerSaving
    );
}
