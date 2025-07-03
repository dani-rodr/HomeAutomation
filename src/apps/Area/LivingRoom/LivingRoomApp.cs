using HomeAutomation.apps.Area.LivingRoom.Automations;

namespace HomeAutomation.apps.Area.LivingRoom;

public class LivingRoomApp(
    ILivingRoomMotionEntities motionEntities,
    ILivingRoomFanEntities fanEntities,
    IAirQualityEntities airQualityEntities,
    ITabletEntities tabletEntities,
    ITclDisplay tclDisplay,
    IScheduler scheduler,
    ILoggerFactory loggerFactory
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
            loggerFactory.CreateLogger<TabletAutomation>()
        );
        yield return new MotionAutomation(
            motionEntities,
            new DimmingLightController(
                motionEntities.SensorDelay,
                scheduler,
                loggerFactory.CreateLogger<DimmingLightController>()
            ),
            loggerFactory.CreateLogger<MotionAutomation>()
        );
    }
}
