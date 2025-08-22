namespace HomeAutomation.apps.Common.Base;

public abstract class ComputerBase(IEventHandler eventHandler, ILogger logger)
    : AutomationBase(logger)
{
    protected abstract string ShowEvent { get; }
    protected abstract string HideEvent { get; }
    public abstract void TurnOn();
    public abstract void TurnOff();
    public abstract IObservable<bool> StateChanges();

    public virtual IObservable<bool> OnShowRequested() =>
        eventHandler.WhenEventTriggered(ShowEvent).Select(_ => true);

    public virtual IObservable<bool> OnHideRequested() =>
        eventHandler.WhenEventTriggered(HideEvent).Select(_ => true);

    public abstract bool IsOn();
}
