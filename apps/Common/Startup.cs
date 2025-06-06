namespace HomeAutomation.apps.Common;

[NetDaemonApp]
public class Startup
{
    public Startup(IHaContext ha)
    {
        ha.CallService("browser_mod", "notification",
            data: new { message = "NetDaemonApp has started", action_text = "Dismiss" });
    }
}