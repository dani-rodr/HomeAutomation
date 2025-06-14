using HomeAutomation.apps.Area.LivingRoom.Automations;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomMotionEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITabletAutomationEntities tabletEntities,
    ILogger<LivingRoomApp> logger
) : AreaBase<LivingRoomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        DimmingLightController dimmingController = new(motionEntities.SensorDelay);

        yield return new MotionAutomation(motionEntities, dimmingController, logger);
        yield return new FanAutomation(fanEntities, logger);
        yield return new AirQualityAutomations(airQualityEntities, logger);
        yield return new TabletAutomations(tabletEntities, logger);
    }
}
