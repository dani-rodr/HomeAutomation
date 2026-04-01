using System.Text.Json.Nodes;

namespace HomeAutomation.apps.Common.Config;

public interface IAreaConfigStore
{
    IReadOnlyCollection<AreaConfigDescriptor> ListAreas();

    JsonObject GetConfig(string areaKey);

    T GetConfig<T>(string areaKey)
        where T : class;

    AreaConfigValidationResult SaveConfig(string areaKey, JsonObject config);

    JsonObject ResetConfig(string areaKey);
}
