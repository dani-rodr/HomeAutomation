namespace HomeAutomation.apps.Common.Services.Logging;

public sealed class AutomationLogger<T>(
    ILoggerFactory loggerFactory,
    IAutomationLogPolicy policy,
    ILogbookSink logbookSink
) : ILogger<T>
{
    private readonly ILogger _innerLogger = loggerFactory.CreateLogger(
        typeof(T).FullName ?? typeof(T).Name
    );
    private readonly string _categoryName = typeof(T).FullName ?? typeof(T).Name;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => _innerLogger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) =>
        _innerLogger.IsEnabled(logLevel) && policy.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        _innerLogger.Log(logLevel, eventId, state, exception, formatter);

        if (policy.ShouldWriteToLogbook(_categoryName, logLevel))
        {
            logbookSink.TryWrite(_categoryName, logLevel, formatter(state, exception), exception);
        }
    }
}
