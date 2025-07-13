using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class MotionSensorRestartScheduler(IScheduler scheduler) : IMotionSensorRestartScheduler
{
    public IEnumerable<IDisposable> GetSchedules(Action action)
    {
        yield return scheduler.ScheduleCron("0 1 * * *", action);
    }
}
