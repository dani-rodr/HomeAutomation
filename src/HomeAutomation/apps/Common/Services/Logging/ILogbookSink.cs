namespace HomeAutomation.apps.Common.Services.Logging;

public interface ILogbookSink
{
    void TryWrite(string categoryName, LogLevel logLevel, string message, Exception? exception);
}
