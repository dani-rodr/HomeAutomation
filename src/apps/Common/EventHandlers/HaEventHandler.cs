using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json;

namespace HomeAutomation.apps.Common.EventHandlers;

public class HaEventHandler(IHaContext haContext, ILogger logger) : IEventHandler, IDisposable
{
    private const string NFC_EVENT = "tag_scanned";
    private readonly CompositeDisposable _disposables = [];

    public IDisposable Subscribe(string eventType, Action<Event> handler)
    {
        var subscription = haContext
            .Events.Where(e => e.EventType == eventType)
            .Subscribe(e =>
            {
                logger.LogInformation("Event '{EventType}' received with data: {Data}", eventType, e.DataElement);
                handler(e);
            });

        _disposables.Add(subscription);
        return subscription;
    }

    public IDisposable Subscribe(string eventType, Action callback)
    {
        return Subscribe(eventType, _ => callback());
    }

    public IObservable<Event> WhenEventTriggered(string eventType)
    {
        return haContext.Events.Where(e => e.EventType == eventType);
    }

    public IObservable<string> OnNfcScan(string tagId)
    {
        return WhenEventTriggered(NFC_EVENT)
            .Where(e =>
            {
                var scannedId = e.DataElement?.GetProperty("tag_id").GetString();
                var match = scannedId == tagId;
                logger.LogInformation("NFC scanned: {Id} (match: {Match})", scannedId ?? "null", match);

                return match;
            })
            .Select(_ => tagId);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposables.Dispose();
        logger.LogInformation("HaEventHandler disposed and subscriptions cleaned up.");
    }
}
