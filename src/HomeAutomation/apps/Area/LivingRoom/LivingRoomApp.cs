using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Automations.Entities;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    MotionSensor motionSensor,
    ILogger<FanAutomation> fanAutomationLogger,
    ILogger<AirQualityAutomation> airQualityAutomationLogger,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<LivingRoomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return tclDisplay;

        yield return new FanAutomation(fanEntities, fanAutomationLogger);

        yield return new AirQualityAutomation(airQualityEntities, airQualityAutomationLogger);

        // yield return new TabletAutomation(tabletEntities, tabletAutomationLogger);

        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay),
            lightAutomationLogger
        );
    }
}
