using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common;

public class HaEventHandler(IHaContext haContext, ILogger logger) : IEventHandler
{
    public void Subscribe(string eventType, Action<Event> handler)
    {
        haContext
            .Events.Where(e => e.EventType == eventType)
            .Subscribe(e =>
            {
                logger.LogInformation("Event '{EventType}' received with data: {Data}", eventType, e.DataElement);
                handler(e);
            });
    }

    public void Subscribe(string eventType, Action callback)
    {
        Subscribe(eventType, _ => callback());
    }

    public IObservable<Event> WhenEventTriggered(string eventType)
    {
        return haContext.Events.Where(e => e.EventType == eventType);
    }
}
