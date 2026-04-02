using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Config;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    IAppConfig<LivingRoomSettings> settings,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    MotionSensor motionSensor,
    IAutomationFactory automationFactory
) : AppBase<LivingRoomSettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return tclDisplay;

        yield return automationFactory.Create<FanAutomation>(fanEntities, Settings.Fan);

        yield return automationFactory.Create<AirQualityAutomation>(
            airQualityEntities,
            Settings.AirQuality
        );

        // yield return new TabletAutomation(tabletEntities, tabletAutomationLogger);

        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(
            motionEntities,
            Settings.Light,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay)
        );
    }
}
