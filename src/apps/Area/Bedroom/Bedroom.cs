using System.Reactive.Concurrency;
using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

[NetDaemonApp]
public class Bedroom
{
    public Bedroom(Entities entities, ILogger<Bedroom> logger, IScheduler scheduler)
    {
        var motionAutomation = new MotionAutomation(entities, logger);
        motionAutomation.StartAutomation();

        var climateAutomation = new ClimateAutomation(entities, scheduler, logger);
        climateAutomation.StartAutomation();
    }
}
