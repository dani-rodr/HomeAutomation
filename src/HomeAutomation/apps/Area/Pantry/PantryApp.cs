using HomeAutomation.apps.Area.Pantry.Automations;
using HomeAutomation.apps.Area.Pantry.Automations.Entities;
using HomeAutomation.apps.Area.Pantry.Config;
using HomeAutomation.apps.Area.Pantry.Devices;

namespace HomeAutomation.apps.Area.Pantry;

public class PantryApp(
    IPantryLightEntities motionEntities,
    MotionSensor motionSensor,
    IAppConfig<PantrySettings> settings,
    IAutomationFactory automationFactory
) : AppBase<PantrySettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(motionEntities, Settings.Light);
    }
}
