using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    MotionSensor motionSensor,
    ILoggerFactory loggerFactory
) : AppBase<LivingRoomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return tclDisplay;
        yield return new FanAutomation(fanEntities, loggerFactory.CreateLogger<FanAutomation>());
        yield return new AirQualityAutomation(
            airQualityEntities,
            loggerFactory.CreateLogger<AirQualityAutomation>()
        );
        // yield return new TabletAutomation(
        //     tabletEntities,
        //     loggerFactory.CreateLogger<TabletAutomation>()
        // );
        yield return motionSensor;

        yield return new LightAutomation(
            motionEntities,
            dimmingLightControllerFactory.Create(motionEntities.SensorDelay),
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
