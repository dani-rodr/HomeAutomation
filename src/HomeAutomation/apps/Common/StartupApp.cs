namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class StartupApp : AppBase<StartupApp>
{
    protected override IEnumerable<IAutomation> CreateAutomations() => [];

    public StartupApp(HomeAssistantGenerated.Services services, ILogger<StartupApp> logger)
        : base()
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
        _ = DismissNotificationLaterAsync(services);
    }

    private static async Task DismissNotificationLaterAsync(
        HomeAssistantGenerated.Services services
    )
    {
        await Task.Delay(TimeSpan.FromSeconds(10));
        services.PersistentNotification.Dismiss(
            new PersistentNotificationDismissParameters { NotificationId = "netdaemon_start" }
        );
    }
}
