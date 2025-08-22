using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Common.Interface;

public interface IAutomationScheduler
{
    IEnumerable<IDisposable> GetSchedules(Action action);
}

public interface ILaptopScheduler : IAutomationScheduler;

public interface IMotionSensorRestartScheduler : IAutomationScheduler;

public interface IClimateScheduler : IAutomationScheduler
{
    IDisposable GetResetSchedule();
    TimeBlock? FindCurrentTimeBlock();
    bool TryGetSetting(TimeBlock timeBlock, [NotNullWhen(true)] out AcSettings? setting);
    int CalculateTemperature(AcSettings settings, bool isOccupied, bool isDoorOpen);
}
