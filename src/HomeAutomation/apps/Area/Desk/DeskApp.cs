using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Automations.Entities;
using HomeAutomation.apps.Area.Desk.Config;
using HomeAutomation.apps.Area.Desk.Devices;
using HomeAutomation.apps.Common.Config;

namespace HomeAutomation.apps.Area.Desk;

[AreaKey("desk")]
public class DeskApp(
    IDeskLightEntities deskMotionEntities,
    IAreaConfigStore areaConfigStore,
    ILgDisplay lgDisplay,
    IDesktop desktop,
    MotionSensor motionSensor,
    ILogger<LightAutomation> lightAutomationLogger,
    ILogger<DisplayAutomation> displayAutomationLogger
) : AppBase<DeskApp, DeskSettings>(areaConfigStore)
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return desktop;

        yield return lgDisplay;

        yield return motionSensor;

        yield return new LightAutomation(
            deskMotionEntities,
            Settings.Light,
            lgDisplay,
            lightAutomationLogger
        );

        yield return new DisplayAutomation(
            lgDisplay,
            desktop,
            deskMotionEntities.MasterSwitch,
            displayAutomationLogger
        );
    }
}
