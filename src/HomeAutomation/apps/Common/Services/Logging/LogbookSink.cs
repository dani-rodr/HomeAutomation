using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Services.Logging;

public sealed class LogbookSink(IServiceScopeFactory scopeFactory) : ILogbookSink
{
    private const string LogbookName = "HomeAutomation";
    private const string LogbookDomain = "automation";
    private static readonly TimeSpan InformationDeduplicationWindow = TimeSpan.FromSeconds(60);
    private readonly object _deduplicationLock = new();
    private readonly Dictionary<string, DateTimeOffset> _lastInformationWriteUtc = [];

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

            if (
                logLevel == LogLevel.Information
                && IsDuplicateInformationEntry(categoryName, details)
            )
            {
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var services = scope.ServiceProvider.GetRequiredService<IServices>();

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

    private bool IsDuplicateInformationEntry(string categoryName, string details)
    {
        var key = $"{categoryName}|{details}";
        var now = DateTimeOffset.UtcNow;

        lock (_deduplicationLock)
        {
            if (
                _lastInformationWriteUtc.TryGetValue(key, out var lastWrite)
                && now - lastWrite < InformationDeduplicationWindow
            )
            {
                return true;
            }

            _lastInformationWriteUtc[key] = now;
            return false;
        }
    }

    private static string ShortCategoryName(string categoryName)
    {
        var lastDot = categoryName.LastIndexOf('.');
        return lastDot < 0 ? categoryName : categoryName[(lastDot + 1)..];
    }
}
