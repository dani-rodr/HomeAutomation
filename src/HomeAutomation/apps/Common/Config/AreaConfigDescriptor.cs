namespace HomeAutomation.apps.Common.Config;

public sealed record AreaConfigDescriptor(
    string Key,
    string Name,
    string Description,
    string DefaultsFilePath,
    string OverridesFilePath,
    string? SchemaFilePath = null
);
