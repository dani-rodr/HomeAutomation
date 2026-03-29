namespace HomeAutomation.apps.Common.Services.Logging;

public sealed class AutomationLogPolicy(InputSelectEntities inputSelectEntities)
    : IAutomationLogPolicy
{
    private const LogLevel DefaultLogLevel = LogLevel.Information;
    private const string AppCategoryPrefix = "HomeAutomation.apps.";

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
        var selectedLevel = inputSelectEntities.AutomationLogLevel.State;
        if (string.IsNullOrWhiteSpace(selectedLevel))
        {
            return DefaultLogLevel;
        }

        return Enum.TryParse<LogLevel>(selectedLevel, ignoreCase: true, out var parsed)
            ? parsed
            : DefaultLogLevel;
    }

    private static bool IsAppCategory(string categoryName) =>
        categoryName.StartsWith(AppCategoryPrefix, StringComparison.Ordinal);

    private static bool IsKeyAutomationCategory(string categoryName) =>
        categoryName.EndsWith("Automation", StringComparison.Ordinal)
        || categoryName.EndsWith("StartupApp", StringComparison.Ordinal);
}
