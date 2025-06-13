using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(Entities entities, ILogger<LivingRoomApp> logger) : AreaBase<LivingRoomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var sharedEntities = new LivingRoomSharedEntities(Entities);

        var motionEntities = new LivingRoomMotionEntities(Entities);
        yield return new MotionAutomation(motionEntities, Logger);

        var fanEntities = new LivingRoomFanEntities(Entities, sharedEntities);
        yield return new FanAutomation(fanEntities, Logger);

        var airQualityEntities = new AirQualityEntities(Entities, sharedEntities);
        yield return new AirQualityAutomations(airQualityEntities, Logger);

        var tabletEntities = new LivingRoomTabletEntities(Entities, sharedEntities);
        yield return new TabletAutomations(tabletEntities, Logger);
    }
}
