namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class Startup
{
    public Startup(IHaContext ha, ILogger<Startup> logger)
    {
        ha.CallService(
            "browser_mod",
            "notification",
            data: new { message = "NetDaemonApp has started", action_text = "Dismiss" }
        );
        logger.LogInformation("NetDaemonApp has started");
    }
}
