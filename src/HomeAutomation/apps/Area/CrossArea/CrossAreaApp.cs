using HomeAutomation.apps.Area.Bedroom.Devices;
using HomeAutomation.apps.Area.CrossArea.Automations;

namespace HomeAutomation.apps.Area.CrossArea;

public class CrossAreaApp(
    ICrossAreaEntities entities,
    ILoggerFactory loggerFactory,
    MotionSensor bedroomMotionSensor
) : AppBase<CrossAreaApp>()
{
    protected override IEnumerable<IAutomation> CreateAutomations()
    {
        yield return new MasterSwitchAutomation(
            entities,
            bedroomMotionSensor,
            loggerFactory.CreateLogger<MasterSwitchAutomation>()
        );
    }
}
