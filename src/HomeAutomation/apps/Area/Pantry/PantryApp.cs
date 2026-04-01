using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Area.Pantry.Config;
using HomeAutomation.apps.Area.Pantry.Devices;

namespace HomeAutomation.apps.Area.Pantry;

[AreaKey("pantry")]
public class PantryApp(
    IPantryLightEntities motionEntities,
    MotionSensor motionSensor,
    IAppConfig<PantrySettings> settings,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<PantryApp, PantrySettings>(settings.Value)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, Settings.Light, lightAutomationLogger);
    }
}
