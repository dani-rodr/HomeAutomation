namespace HomeAutomation.apps.Common.Interface;

public interface ILaptop : IComputer;

public interface IDesktop : IComputer;

public interface IComputer : IAutomation
{
    void TurnOn();
    void TurnOff();
    IObservable<bool> StateChanges();
    IObservable<bool> OnShowRequested();
    IObservable<bool> OnHideRequested();
    bool IsOn();
}
