using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;
using HomeAutomation.apps.Area.LivingRoom.Devices;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.LivingRoom;

[AreaKey("livingroom")]
public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    IAreaConfigStore areaConfigStore,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    MotionSensor motionSensor,
    ILogger<FanAutomation> fanAutomationLogger,
    ILogger<AirQualityAutomation> airQualityAutomationLogger,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<LivingRoomApp, LivingRoomSettings>(areaConfigStore)
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
