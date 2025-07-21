using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITabletEntities tabletEntities,
    ITclDisplay tclDisplay,
    IDimmingLightControllerFactory dimmingLightControllerFactory,
    Devices.MotionSensor motionSensor,
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
        yield return motionSensor;

        var motionAutomation = new MotionAutomation(
            motionSensor,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
        yield return motionAutomation;

        yield return new TabletAutomation(
            tabletEntities,
            motionAutomation,
            loggerFactory.CreateLogger<TabletAutomation>()
        );

        yield return new LightAutomation(
            motionEntities,
            motionAutomation,
            dimmingLightControllerFactory.Create(motionSensor.SensorDelay),
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
