namespace HomeAutomation.apps.Common.Services;

public class WebhookServices(ITriggerManager manager, ILogger<WebhookServices> logger)
    : IWebhookServices
{
    private readonly string[] defaultMethods = ["POST", "PUT"];
    private Dictionary<string, IDisposable> _subscriptions = [];

    public bool Register(
        string webhookId,
        Action<object?> onTriggered,
        string[]? allowedMethods = null,
        bool localOnly = true
    )
    {
        if (_subscriptions.ContainsKey(webhookId))
        {
            logger.LogWarning("Webhook '{WebhookId}' already registered. Skipping.", webhookId);
            return false;
        }
        allowedMethods ??= defaultMethods;
        var observable = manager.RegisterTrigger(
            new
            {
                platform = "webhook",
                webhook_id = webhookId,
                allowed_methods = allowedMethods,
                local_only = localOnly,
            }
        );

        _subscriptions[webhookId] = observable.Subscribe(trigger =>
        {
            logger.LogDebug(
                "Webhook '{WebhookId}' received with payload: {@Payload}",
                webhookId,
                trigger
            );
            onTriggered?.Invoke(trigger);
        });

        logger.LogDebug("Webhook '{WebhookId}' registered.", webhookId);
        return true;
    }

    public bool Unregister(string webhookId)
    {
        if (!_subscriptions.Remove(webhookId, out var subscription))
        {
            logger.LogWarning("Webhook '{WebhookId}' was not registered.", webhookId);

            return false;
        }
        subscription.Dispose();
        logger.LogInformation("Webhook '{WebhookId}' unregistered.", webhookId);
        return true;
    }

    public void Dispose()
    {
        foreach (var (webhookId, subscription) in _subscriptions)
        {
            subscription.Dispose();
            logger.LogDebug("Webhook '{WebhookId}' disposed.", webhookId);
        }
        GC.SuppressFinalize(this);
    }
}
