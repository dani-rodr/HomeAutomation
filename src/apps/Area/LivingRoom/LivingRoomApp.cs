using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Area.LivingRoom.Devices;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomLightEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITabletEntities tabletEntities,
    ITclDisplay tclDisplay,
    IScheduler scheduler,
    ILoggerFactory loggerFactory,
    ITypedEntityFactory entityFactory
) : AppBase<LivingRoomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        tclDisplay.StartAutomation();
        yield return new FanAutomation(fanEntities, loggerFactory.CreateLogger<FanAutomation>());
        yield return new AirQualityAutomation(
            airQualityEntities,
            loggerFactory.CreateLogger<AirQualityAutomation>()
        );
        yield return new TabletAutomation(
            tabletEntities,
            scheduler,
            loggerFactory.CreateLogger<TabletAutomation>()
        );
        yield return new MotionSensor(entityFactory, loggerFactory.CreateLogger<MotionSensor>());

        yield return new LightAutomation(
            motionEntities,
            new DimmingLightController(
                motionEntities.SensorDelay,
                scheduler,
                loggerFactory.CreateLogger<DimmingLightController>()
            ),
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
    }
}
