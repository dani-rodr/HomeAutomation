namespace HomeAutomation.apps.Common.Services;

public class NotificationServices(IServices services, ILogger logger) : INotificationServices
{
    private readonly NotifyServices _notifyServices = services.Notify;

    public void LaunchAppPocoF4(string packageName)
    {
        logger.LogInformation("Sending 'command_launch_app' to PocoF4 GT for package: {PackageName}", packageName);
        _notifyServices.MobileAppPocoF4Gt(
            new() { Message = "command_launch_app", Data = new { package_name = packageName } }
        );
    }

    public void LaunchAppMiPad(string packageName)
    {
        logger.LogInformation("Sending 'command_launch_app' to MiPad for package: {PackageName}", packageName);
        _notifyServices.MobileApp21051182c(
            new() { Message = "command_launch_app", Data = new { package_name = packageName } }
        );
    }
}
