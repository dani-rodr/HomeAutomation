namespace HomeAutomation.apps.Common.Services;

public class NotificationServices(IServices services, ILogger<NotificationServices> logger)
    : INotificationServices
{
    private readonly NotifyServices _notifyServices = services.Notify;

    public void NotifyPocoF4(string message, object data, string? title = null)
    {
        _notifyServices.MobileAppPocoF4Gt(
            new()
            {
                Title = title,
                Message = message,
                Data = data,
            }
        );
    }

    public void NotifyMiPad(string message, object data, string? title = null)
    {
        _notifyServices.MobileApp21051182c(
            new()
            {
                Title = title,
                Message = message,
                Data = data,
            }
        );
    }

    public void LaunchAppPocoF4(string packageName)
    {
        logger.LogDebug(
            "Sending 'command_launch_app' to PocoF4 GT for package: {PackageName}",
            packageName
        );
        NotifyPocoF4("command_launch_app", new { package_name = packageName });
    }

    public void LaunchAppMiPad(string packageName)
    {
        logger.LogDebug(
            "Sending 'command_launch_app' to MiPad for package: {PackageName}",
            packageName
        );
        NotifyPocoF4("command_launch_app", new { package_name = packageName });
    }
}
