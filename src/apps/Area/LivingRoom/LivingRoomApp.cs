using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(Entities entities, ILogger<LivingRoomApp> logger) : AreaBase<LivingRoomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        var standFan = Entities.Switch.Sonoff10023810231;
        yield return new MotionAutomation(Entities, Logger);
        yield return new FanAutomation(Entities, standFan, Logger);
        yield return new AirQualityAutomations(Entities, standFan, Logger);
    }
}
