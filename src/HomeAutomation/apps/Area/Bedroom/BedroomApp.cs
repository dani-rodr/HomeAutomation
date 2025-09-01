using HomeAutomation.apps.Area.Bathroom.Devices;
using HomeAutomation.apps.Area.Bedroom.Automations;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILoggerFactory loggerFactory,
    IBedroomLightEntities motionEntities,
    IBedroomFanEntities fanEntities,
    IClimateEntities climateEntities,
    IClimateScheduler climateScheduler,
    MotionSensor motionSensor
) : AppBase<BedroomApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;
        yield return new LightAutomation(
            motionEntities,
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
