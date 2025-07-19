namespace HomeAutomation.apps.Common.Interface;

public interface IWebhookServices : IDisposable
{
    bool Register(
        string webhookId,
        Action<object?> onTriggered,
        string[]? allowedMethods = null,
        bool localOnly = true
    );
    bool Unregister(string webhookId);
}
