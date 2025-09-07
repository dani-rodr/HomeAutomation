namespace HomeAutomation.apps.Helpers.Extensions;

public static class LoggerExtensions
{
    public static void LogDebug(
        this ILogger logger,
        string message,
        bool logToHaLogbook = false,
        params object?[] args
    )
    {
        if (logToHaLogbook)
        {
            // Do something extra
        }
        logger.LogDebug(message, args);
    }
}
// Smooth Option: Custom ILoggerProvider

// You can write a provider that intercepts logs and forwards them to both:

// The underlying Microsoft logger (Console, Serilog, etc.)

// Home Assistant’s Logbook via IServices.

// Example sketch:
// public class HaLoggerProvider : ILoggerProvider
// {
//     private readonly ILoggerProvider _inner;
//     private readonly IServices _haServices;

//     public HaLoggerProvider(ILoggerProvider inner, IServices haServices)
//     {
//         _inner = inner;
//         _haServices = haServices;
//     }

//     public ILogger CreateLogger(string categoryName)
//     {
//         var innerLogger = _inner.CreateLogger(categoryName);
//         return new HaLogger(innerLogger, _haServices, categoryName);
//     }

//     public void Dispose() => _inner.Dispose();
// }

// public class HaLogger : ILogger
// {
//     private readonly ILogger _inner;
//     private readonly IServices _haServices;
//     private readonly string _categoryName;

//     public HaLogger(ILogger inner, IServices haServices, string categoryName)
//     {
//         _inner = inner;
//         _haServices = haServices;
//         _categoryName = categoryName;
//     }

//     public IDisposable BeginScope<TState>(TState state) => _inner.BeginScope(state);

//     public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

//     public void Log<TState>(
//         LogLevel logLevel,
//         EventId eventId,
//         TState state,
//         Exception? exception,
//         Func<TState, Exception?, string> formatter
//     )
//     {
//         var message = formatter(state, exception);

//         // Forward to regular logger
//         _inner.Log(logLevel, eventId, state, exception, formatter);

//         // Extra: also log to HA Logbook for certain levels
//         if (logLevel >= LogLevel.Information)
//         {
//             _haServices.Logbook.Log(name: _categoryName, message: message);
//         }
//     }
// }

// Then, in startup / DI config:
// services.AddLogging(builder =>
// {
//     builder.ClearProviders();
//     builder.AddProvider(new HaLoggerProvider(new ConsoleLoggerProvider(...), haServices));
// });
