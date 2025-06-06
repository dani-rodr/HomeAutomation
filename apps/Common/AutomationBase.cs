using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common;

public abstract class AutomationBase(ILogger logger) : IAutomation
{
    protected ILogger Logger { get; } = logger;

    public abstract void StartAutomation();
}
