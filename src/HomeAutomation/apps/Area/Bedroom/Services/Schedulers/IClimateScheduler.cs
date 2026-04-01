using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

public interface IClimateScheduler : IAutomationScheduler
{
    IDisposable GetResetSchedule();
    TimeBlock? FindCurrentTimeBlock();
    bool TryGetSetting(TimeBlock timeBlock, [NotNullWhen(true)] out AcSettings? setting);
    int CalculateTemperature(AcSettings settings, bool isOccupied, bool isDoorOpen);
}
