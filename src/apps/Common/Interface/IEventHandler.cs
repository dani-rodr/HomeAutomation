using System.Reactive;

namespace HomeAutomation.apps.Common.Interface;

public interface IEventHandler
{
    IDisposable Subscribe(string eventType, Action<Event> handler);
    IDisposable Subscribe(string eventType, Action callback);
    IObservable<Event> WhenEventTriggered(string eventType);
    IObservable<string> OnNfcScan(string tagId);
    IObservable<string> OnMobileEvent(string action);
}
