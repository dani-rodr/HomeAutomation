using System.Collections.Generic;
using HomeAutomation.apps.Area.LivingRoom.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(Entities entities, ILogger<LivingRoomApp> logger) : AreaBase<LivingRoomApp>(entities, logger)
{
    protected override IEnumerable<IAutomation> CreateAutomations() =>
        [new MotionAutomation(Entities, Logger), new FanAutomation(Entities, Logger)];
}
