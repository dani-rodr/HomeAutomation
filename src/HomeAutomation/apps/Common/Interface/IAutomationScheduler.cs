namespace HomeAutomation.apps.Common.Interface;

public interface IAutomationScheduler
{
    IEnumerable<IDisposable> GetSchedules(Action action);
}

public interface ILaptopShutdownScheduler : IAutomationScheduler;

public interface IMotionSensorRestartScheduler : IAutomationScheduler;
