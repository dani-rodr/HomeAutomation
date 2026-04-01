using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

[AreaKey("livingroom")]
public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    IAppConfig<LivingRoomSettings> settings,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    MotionSensor motionSensor,
    ILogger<FanAutomation> fanAutomationLogger,
    ILogger<AirQualityAutomation> airQualityAutomationLogger,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<LivingRoomApp, LivingRoomSettings>(settings.Value)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return tclDisplay;

        yield return new FanAutomation(fanEntities, Settings.Fan, fanAutomationLogger);

        yield return new AirQualityAutomation(
            airQualityEntities,
            Settings.AirQuality,
            airQualityAutomationLogger
        );

        // yield return new TabletAutomation(tabletEntities, tabletAutomationLogger);

        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            Settings.Light,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay),
            lightAutomationLogger
        );
    }
}
