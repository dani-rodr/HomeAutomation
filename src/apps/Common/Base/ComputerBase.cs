using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common.Base;

public abstract class ComputerBase(ILogger logger) : IComputer
{
    protected ILogger Logger = logger;
    public abstract void TurnOn();
    public abstract void TurnOff();
}
