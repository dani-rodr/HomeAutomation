using System.Collections.Generic;
using System.Reactive.Concurrency;
using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(Entities entities, ILogger<BedroomApp> logger, IScheduler scheduler)
    : AreaBase<BedroomApp>(entities, logger, scheduler)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionAutomation(Entities, Logger);
        yield return new ClimateAutomation(Entities, Scheduler!, Logger);
    }
}
