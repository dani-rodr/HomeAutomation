using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Config;
using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    IBedroomLightEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IAppConfig<BedroomSettings> settings,
    IClimateSettingsResolver climateSettingsResolver,
    MotionSensor motionSensor,
    IAutomationFactory automationFactory
) : AppBase<BedroomSettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(motionEntities, Settings.Light);

        yield return automationFactory.Create<FanAutomation>(fanEntities);

        yield return automationFactory.Create<ClimateAutomation>(
            climateEntities,
            climateSettingsResolver
        );
    }
}
