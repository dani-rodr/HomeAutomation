using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Area.Pantry.Devices;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(
    IPantryLightEntities motionEntities,
    MotionSensor motionSensor,
    ILogger<LightAutomation> lightAutomationLogger
) : AppBase<PantryApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return new LightAutomation(motionEntities, lightAutomationLogger);
    }
}
