using HomeAutomation.apps.Area.CrossArea.Automations;
using HomeAutomation.apps.Common.Containers;

namespace HomeAutomation.Tests.Area.CrossArea.Automations;

public class MasterSwitchAutomationTests
{
    private readonly MasterSwitchAutomation _automation;
    private readonly MockHaContext _mockHaContext = new();
    private readonly Mock<ILogger<MasterSwitchAutomation>> _mockLogger = new();
    private readonly TestEntities _testEntities;

    public MasterSwitchAutomationTests()
    {
        _testEntities = new(_mockHaContext);
        _automation = new(_testEntities, _mockBedroomMotionSensor.Object, _mockLogger.Object);
    }

    private class TestEntities(IHaContext haContext) : ICrossAreaEntities
    {
        public SwitchEntity PantryAutomation => new SwitchEntity(haContext, "pantry_automation");
    }
}
