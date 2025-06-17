using NetDaemon.Extensions.Scheduler;

namespace HomeAutomation.apps.Common.Services;

public class LaptopScheduler(ILaptopSchedulerEntities entities, IScheduler scheduler) : ILaptopScheduler
{
    public IEnumerable<IDisposable> GetSchedules(Action action)
    {
        var isProjectNation = entities.ProjectNationWeek;
        yield return scheduler.ScheduleCron("0 17 * * 1-5", action);
        yield return scheduler.ScheduleCron(
            "0 12 * * 5",
            () =>
            {
                if (isProjectNation.IsOn())
                {
                    action();
                }
                isProjectNation.Toggle();
            }
        );
    }
}
