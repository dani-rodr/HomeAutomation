using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Bedroom.Config;

public interface IClimateSettingsProvider
{
    ClimateSettings GetSettings();
}

public sealed class ClimateSettingsProvider(IAreaConfigStore areaConfigStore)
    : IClimateSettingsProvider
{
    private const string BedroomAreaKey = "bedroom";

    public ClimateSettings GetSettings() =>
        areaConfigStore.GetConfig<ClimateSettings>(BedroomAreaKey);
}
