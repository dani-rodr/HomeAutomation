using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common.Base;

public abstract class ComputerBase(ILogger logger) : IComputer
{
    public abstract void TurnOn();
    public abstract void TurnOff();
    public abstract IObservable<bool> StateChanges();
    public abstract IObservable<bool> OnShowRequested();
    public abstract IObservable<bool> OnHideRequested();
    public abstract bool IsOn();
    protected ILogger Logger = logger;
}
