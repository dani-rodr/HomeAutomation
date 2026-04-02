using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Automations.Entities;
using HomeAutomation.apps.Area.Desk.Config;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(
    IDeskLightEntities deskMotionEntities,
    IAppConfig<DeskSettings> settings,
    ILgDisplay lgDisplay,
    IDesktop desktop,
    MotionSensor motionSensor,
    IAutomationFactory automationFactory
) : AppBase<DeskSettings>(settings)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return desktop;

        yield return lgDisplay;

        yield return motionSensor;

        yield return automationFactory.Create<LightAutomation>(
            deskMotionEntities,
            Settings.Light,
            lgDisplay
        );

        yield return automationFactory.Create<DisplayAutomation>(
            lgDisplay,
            desktop,
            deskMotionEntities.MasterSwitch
        );
    }
}
