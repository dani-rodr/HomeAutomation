using Microsoft.Extensions.DependencyInjection;

namespace HomeAutomation.apps.Common.Services.Logging;

public sealed class AutomationLogPolicy(IServiceScopeFactory scopeFactory) : IAutomationLogPolicy
{
    private const LogLevel DefaultLogLevel = LogLevel.Debug;
    private const string AppCategoryPrefix = "HomeAutomation.apps.";
    private static readonly TimeSpan LogLevelCacheDuration = TimeSpan.FromSeconds(5);
    private static readonly HashSet<string> InformationLogbookCategories =
    [
        "HomeAutomation.apps.Common.StartupApp",
        "HomeAutomation.apps.Area.Bedroom.Automations.ClimateAutomation",
        "HomeAutomation.apps.Security.Automations.AccessControlAutomation",
        "HomeAutomation.apps.Security.Automations.LockAutomation",
    ];
    private readonly object _cacheLock = new();
    private DateTimeOffset _lastLevelReadUtc = DateTimeOffset.MinValue;
    private LogLevel _cachedLevel = DefaultLogLevel;

    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
        {
            return false;
        }

        var selectedLevel = ResolveSelectedLevel();
        return selectedLevel != LogLevel.None && logLevel >= selectedLevel;
    }

    public bool ShouldWriteToLogbook(string categoryName, LogLevel logLevel)
    {
        if (!IsAppCategory(categoryName))
        {
            return false;
        }

        return logLevel switch
        {
            >= LogLevel.Warning => true,
            LogLevel.Information => IsKeyAutomationCategory(categoryName),
            _ => false,
        };
    }

    private LogLevel ResolveSelectedLevel()
    {
        var now = DateTimeOffset.UtcNow;

        lock (_cacheLock)
        {
            if (now - _lastLevelReadUtc < LogLevelCacheDuration)
            {
                return _cachedLevel;
            }
        }

        var selectedLevel = ReadSelectedLevelState();
        var resolved =
            string.IsNullOrWhiteSpace(selectedLevel) ? DefaultLogLevel
            : Enum.TryParse<LogLevel>(selectedLevel, ignoreCase: true, out var parsed) ? parsed
            : DefaultLogLevel;

        lock (_cacheLock)
        {
            _cachedLevel = resolved;
            _lastLevelReadUtc = now;
        }

        return resolved;
    }

    private string? ReadSelectedLevelState()
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var inputSelectEntities =
                scope.ServiceProvider.GetRequiredService<InputSelectEntities>();
            return inputSelectEntities.AutomationLogLevel.State;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsAppCategory(string categoryName) =>
        categoryName.StartsWith(AppCategoryPrefix, StringComparison.Ordinal);

    private static bool IsKeyAutomationCategory(string categoryName) =>
        InformationLogbookCategories.Contains(categoryName);
}
