using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Area.Desk.Devices;

namespace HomeAutomation.apps.Area.Desk;

public class DeskApp(
    IEventHandler eventHandler,
    IDeskLightEntities deskMotionEntities,
    ILgDisplay lgDisplay,
    IDesktop desktop,
    ILaptop laptop,
    MotionSensor motionSensor,
    ILoggerFactory loggerFactory
) : AppBase<DeskApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return desktop;
        yield return laptop;
        yield return lgDisplay;
        yield return motionSensor;

        yield return new LightAutomation(
            deskMotionEntities,
            lgDisplay,
            loggerFactory.CreateLogger<LightAutomation>()
        );
        yield return new DisplayAutomation(
            lgDisplay,
            desktop,
            laptop,
            eventHandler,
            deskMotionEntities.MasterSwitch,
            loggerFactory.CreateLogger<DisplayAutomation>()
        );
    }
}
