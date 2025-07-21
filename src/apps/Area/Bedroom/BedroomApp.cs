using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Devices;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILoggerFactory loggerFactory,
    IBedroomLightEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IClimateScheduler climateScheduler,
    Devices.MotionSensor motionSensor,
    IScheduler scheduler
) : AppBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        var motionAutomation = new MotionAutomation(
            motionSensor,
            loggerFactory.CreateLogger<MotionAutomation>()
        );
        yield return motionAutomation;

        yield return new LightAutomation(
            motionEntities,
            motionAutomation,
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
