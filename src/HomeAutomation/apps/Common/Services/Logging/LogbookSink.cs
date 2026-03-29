namespace HomeAutomation.apps.Common.Services.Logging;

public sealed class LogbookSink(IServices services) : ILogbookSink
{
    private const string LogbookName = "HomeAutomation";
    private const string LogbookDomain = "automation";

    public void TryWrite(
        string categoryName,
        LogLevel logLevel,
        string message,
        Exception? exception
    )
    {
        try
        {
            var details = exception is null
                ? message
                : $"{message} | Exception: {exception.GetType().Name} - {exception.Message}";
            services.Logbook.Log(
                LogbookName,
                $"[{logLevel}] {ShortCategoryName(categoryName)}: {details}",
                domain: LogbookDomain
            );
        }
        catch
        {
            // Best-effort sink: never break application flow if logbook service fails.
        }
    }

    private static string ShortCategoryName(string categoryName)
    {
        var lastDot = categoryName.LastIndexOf('.');
        return lastDot < 0 ? categoryName : categoryName[(lastDot + 1)..];
    }
}
