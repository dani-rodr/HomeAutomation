namespace HomeAutomation.apps.Common.Interface;

public interface IEventHandler
{
    void Subscribe(string eventType, Action<Event> handler);
    void Subscribe(string eventType, Action callback);
    IObservable<Event> WhenEventTriggered(string eventType);
}