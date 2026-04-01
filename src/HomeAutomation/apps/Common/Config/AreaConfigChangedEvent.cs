namespace HomeAutomation.apps.Common.Config;

public enum AreaConfigChangeType
{
    Saved,
    Reset,
}

public sealed record AreaConfigChangedEvent(
    string AreaKey,
    AreaConfigChangeType ChangeType,
    DateTimeOffset OccurredAtUtc
);
