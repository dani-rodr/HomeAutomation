namespace HomeAutomation.apps.Common.Interface;

public interface IComputer : IAutomationDevice
{
    void TurnOn();
    void TurnOff();
    IObservable<bool> StateChanges();
    IObservable<bool> OnShowRequested();
    IObservable<bool> OnHideRequested();
    bool IsOn();
}
