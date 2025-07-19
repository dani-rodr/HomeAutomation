using HomeAutomation.apps.Area.Bathroom.Devices;
using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILoggerFactory loggerFactory,
    IBedroomLightEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IClimateScheduler climateScheduler,
    ITypedEntityFactory entityFactory,
    IScheduler scheduler
) : AppBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MotionSensor(entityFactory, loggerFactory.CreateLogger<MotionSensor>());
        yield return new LightAutomation(
            motionEntities,
            scheduler,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new FanAutomation(fanEntities, loggerFactory.CreateLogger<FanAutomation>());
        yield return new ClimateAutomation(
            climateEntities,
            climateScheduler,
            loggerFactory.CreateLogger<ClimateAutomation>()
        );
    }
}
