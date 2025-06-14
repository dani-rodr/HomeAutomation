using HomeAutomation.apps.Area.LivingRoom.Automations;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomMotionEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITabletAutomationEntities tabletEntities,
    ILogger<LivingRoomApp> logger
) : AppBase<LivingRoomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new FanAutomation(fanEntities, logger);
        yield return new AirQualityAutomations(airQualityEntities, logger);
        yield return new TabletAutomations(tabletEntities, logger);
        yield return new MotionAutomation(
            motionEntities,
            new DimmingLightController(motionEntities.SensorDelay),
            logger
        );
    }
}
