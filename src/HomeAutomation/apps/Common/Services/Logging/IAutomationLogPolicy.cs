namespace HomeAutomation.apps.Common.Services.Logging;

public interface IAutomationLogPolicy
{
    bool IsEnabled(LogLevel logLevel);

    bool ShouldWriteToLogbook(string categoryName, LogLevel logLevel);
}
