namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class StartupApp : AppBase<StartupApp>
{
    protected override IEnumerable<IAutomation> CreateAutomations() => [];

    public StartupApp(HomeAssistantGenerated.Services services, ILogger<StartupApp> logger)
        : base()
    {
        services.BrowserMod.Notification(new() { Message = "NetDaemonApp has started", ActionText = "Dismiss" });
        logger.LogInformation("NetDaemonApp has started");
    }
}
