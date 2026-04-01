using System.Text.Json.Nodes;

namespace HomeAutomation.apps.Common.Settings;

public interface IAreaSettingsStore
{
    IReadOnlyCollection<AreaSettingsDescriptor> ListAreas();

    JsonObject GetSettings(string areaKey);

    T GetSettings<T>(string areaKey)
        where T : class;

    object GetSettings(string areaKey, Type settingsType);

    AreaSettingsValidationResult SaveSettings(string areaKey, JsonObject settings);

    JsonObject ResetSettings(string areaKey);
}
