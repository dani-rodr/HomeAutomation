using System.Reactive.Disposables;
using System.Text.Json;

namespace HomeAutomation.apps.Common.Services;

public class HaEventHandler(IHaContext haContext, ILogger<HaEventHandler> logger) : IEventHandler, IDisposable
{
    private const string NFC_EVENT = "tag_scanned";
    private const string MOBILE_EVENT = "mobile_app_notification_action";

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

    public IObservable<string> OnNfcScan(string tagId) =>
        MatchEventByProperty(NFC_EVENT, "tag_id", tagId, logLabel: "NFC scanned");

    public IObservable<string> OnMobileEvent(string action) => MatchEventByProperty(MOBILE_EVENT, "action", action);

    private IObservable<string> MatchEventByProperty(
        string eventType,
        string propertyName,
        string expectedValue,
        string? logLabel = null
    ) =>
        WhenEventTriggered(eventType)
            .Where(e =>
            {
                var value = e.DataElement?.GetProperty(propertyName).GetString();
                var isMatch = value == expectedValue;

                if (logLabel is not null)
                    logger.LogInformation("{Label}: {Value} (match: {Match})", logLabel, value ?? "null", isMatch);

                return isMatch;
            })
            .Select(e =>
            {
                if (
                    e.DataElement is JsonElement data
                    && data.TryGetProperty("context", out var context)
                    && context.TryGetProperty("user_id", out var userId)
                )
                {
                    return userId.GetString() ?? string.Empty;
                }

                return string.Empty;
            });

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposables.Dispose();
        logger.LogInformation("HaEventHandler disposed and subscriptions cleaned up.");
    }
}
