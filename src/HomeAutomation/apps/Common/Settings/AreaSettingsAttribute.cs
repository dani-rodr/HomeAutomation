namespace HomeAutomation.apps.Common.Settings;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AreaSettingsAttribute(string key, string name, string description) : Attribute
{
    public string Key { get; } = key;

    public string Name { get; } = name;

    public string Description { get; } = description;
}
