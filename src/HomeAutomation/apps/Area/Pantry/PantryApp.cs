using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Area.Pantry.Config;
using HomeAutomation.apps.Area.Pantry.Devices;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Pantry;

[AreaKey("pantry")]
public class PantryApp(
    IPantryLightEntities motionEntities,
    MotionSensor motionSensor,
    IAreaConfigStore areaConfigStore,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<PantryApp, PantrySettings>(areaConfigStore)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, Settings.Light, lightAutomationLogger);
    }
}
