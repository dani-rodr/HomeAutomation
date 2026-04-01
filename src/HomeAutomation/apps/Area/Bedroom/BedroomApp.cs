using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Bedroom;

[AreaKey("bedroom")]
public class BedroomApp(
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<FanAutomation> fanAutomationLogger,
    ILogger<ClimateAutomation> climateAutomationLogger,
    IBedroomLightEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IAreaConfigStore areaConfigStore,
    IClimateSettingsResolver climateSettingsResolver,
    IAreaConfigChangeNotifier areaConfigChangeNotifier,
    MotionSensor motionSensor
) : AppBase<BedroomApp, ClimateSettings>(areaConfigStore)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, Settings.Light, lightAutomationLogger);

        yield return new FanAutomation(fanEntities, fanAutomationLogger);

        yield return new ClimateAutomation(
            climateEntities,
            climateSettingsResolver,
            areaConfigChangeNotifier,
            climateAutomationLogger
        );
    }
}
