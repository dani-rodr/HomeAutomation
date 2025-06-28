using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Common.Interface;

public interface IAutomationScheduler
{
    IEnumerable<IDisposable> GetSchedules(Action action);
}

public interface ILaptopScheduler : IAutomationScheduler;

public interface IClimateScheduler : IAutomationScheduler
{
    IDisposable GetResetSchedule();
    TimeBlock? FindCurrentTimeBlock();
    void LogCurrentAcScheduleSettings();
    bool TryGetSetting(TimeBlock timeBlock, [NotNullWhen(true)] out AcScheduleSetting? setting);
}
