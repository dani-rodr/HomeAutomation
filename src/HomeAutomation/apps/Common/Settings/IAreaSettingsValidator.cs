namespace HomeAutomation.apps.Common.Settings;

public interface IAreaSettingsValidator
{
    IReadOnlyDictionary<string, string[]> Validate(object settings);
}
