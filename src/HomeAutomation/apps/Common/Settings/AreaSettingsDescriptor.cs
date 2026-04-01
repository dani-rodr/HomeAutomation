namespace HomeAutomation.apps.Common.Settings;

public sealed record AreaSettingsDescriptor(
    string Key,
    string Name,
    string Description,
    Type SettingsType,
    string SettingsFilePath
)
{
    public string SettingsSectionKey => SettingsType.FullName ?? SettingsType.Name;
}
