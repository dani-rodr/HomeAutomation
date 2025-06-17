namespace HomeAutomation.apps.Common.Interface;

public interface ILaptopScheduler
{
    IEnumerable<IDisposable> GetSchedules(Action action);
}
