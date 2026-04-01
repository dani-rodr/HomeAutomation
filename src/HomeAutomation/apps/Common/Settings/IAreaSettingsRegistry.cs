namespace HomeAutomation.apps.Common.Settings;

public interface IAreaSettingsRegistry
{
    IReadOnlyCollection<AreaSettingsDescriptor> List();

    bool TryGet(string areaKey, out AreaSettingsDescriptor descriptor);
}
