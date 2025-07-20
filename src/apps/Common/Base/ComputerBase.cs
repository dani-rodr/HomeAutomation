namespace HomeAutomation.apps.Common.Base;

public abstract class ComputerBase(IEventHandler eventHandler, ILogger logger)
    : AutomationDeviceBase
{
    protected abstract string ShowEvent { get; }
    protected abstract string HideEvent { get; }

    protected readonly ILogger Logger = logger;
    public abstract void TurnOn();
    public abstract void TurnOff();
    public abstract IObservable<bool> StateChanges();

    public virtual IObservable<bool> OnShowRequested() =>
        eventHandler.WhenEventTriggered(ShowEvent).Select(_ => true);

    public virtual IObservable<bool> OnHideRequested() =>
        eventHandler.WhenEventTriggered(HideEvent).Select(_ => true);

    public abstract bool IsOn();
}
