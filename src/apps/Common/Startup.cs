namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class Startup
{
    public Startup(IHaContext ha, IServices services, ILogger<Startup> logger)
    {
        services.BrowserMod.Notification(new() { Message = "NetDaemonApp has started", ActionText = "Dismiss" });
        logger.LogInformation("NetDaemonApp has started");
    }
}
