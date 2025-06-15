namespace HomeAutomation.apps.Common.Interface;

public interface INotificationServices
{
    public void NotifyPocoF4(string message, object data, string? title = null);
    public void NotifyMiPad(string message, object data, string? title = null);
    public void LaunchAppPocoF4(string packageName);
    public void LaunchAppMiPad(string packageName);
}
