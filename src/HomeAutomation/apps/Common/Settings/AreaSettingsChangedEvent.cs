namespace HomeAutomation.apps.Common.Settings;

public enum AreaSettingsChangeType
{
    Saved,
    Reset,
}

public sealed record AreaSettingsChangedEvent(
    string AreaKey,
    AreaSettingsChangeType ChangeType,
    DateTimeOffset OccurredAtUtc
);
