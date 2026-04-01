using HomeAutomation.apps.Area.Bedroom.Automations;
using HomeAutomation.apps.Area.Bedroom.Automations.Entities;
using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Area.Bedroom.Services.Schedulers;

namespace HomeAutomation.apps.Area.Bedroom;

public class BedroomApp(
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<FanAutomation> fanAutomationLogger,
    ILogger<ClimateAutomation> climateAutomationLogger,
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

        yield return new LightAutomation(motionEntities, lightAutomationLogger);

        yield return new FanAutomation(fanEntities, fanAutomationLogger);

        yield return new ClimateAutomation(
            climateEntities,
            climateScheduler,
            climateAutomationLogger
        );
    }
}
