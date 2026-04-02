namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class StartupApp : AppBase<NoAppSettings>
{
    private static readonly TimeSpan DefaultNotificationDismissDelay = TimeSpan.FromSeconds(10);

    protected override IEnumerable<IAutomation> CreateAutomations() => [];

    public StartupApp(
        HomeAssistantGenerated.Services services,
        ILogger<StartupApp> logger,
        IAppConfig<NoAppSettings> settings
    )
        : base(settings)
    {
        services.BrowserMod.Notification(
            new() { Message = "NetDaemonApp has started", ActionText = "Dismiss" }
        );
        logger.LogDebug("NetDaemon V6 has started");

        services.PersistentNotification.Create(
            new PersistentNotificationCreateParameters
            {
                Title = "NetDaemon",
                Message =
                    "Automation Started : View [Logs](/hassio/addon/c6a2317c_netdaemon6/logs)",
                NotificationId = "netdaemon_start",
            }
        );

        _ = DismissNotificationLaterAsync(
            () =>
                services.PersistentNotification.Dismiss(
                    new PersistentNotificationDismissParameters
                    {
                        NotificationId = "netdaemon_start",
                    }
                ),
            logger,
            DefaultNotificationDismissDelay
        );
    }

    public static async Task DismissNotificationLaterAsync(
        Action dismissNotification,
        ILogger logger,
        TimeSpan delay,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            dismissNotification();
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Notification dismissal was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to dismiss startup notification.");
        }
    }
}
