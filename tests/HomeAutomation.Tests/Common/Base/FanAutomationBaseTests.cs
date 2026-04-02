using HomeAutomation.apps.Common.Base;
using HomeAutomation.apps.Common.Contracts;

namespace HomeAutomation.Tests.Common.Base;

public class FanAutomationBaseTests : HaContextTestBase
{
    [Fact]
    public void Constructor_WhenFanCollectionIsEmpty_ShouldThrowClearConfigurationException()
    {
        // Arrange
        var entities = new Mock<IFanAutomationEntities>();
        entities
            .SetupGet(x => x.MasterSwitch)
            .Returns(new SwitchEntity(HaContext, "switch.fan_master"));
        entities
            .SetupGet(x => x.MotionSensor)
            .Returns(new BinarySensorEntity(HaContext, "binary_sensor.fan_motion"));
        entities.SetupGet(x => x.Fans).Returns([]);
        var logger = new Mock<ILogger>();

        // Act
        var act = () => _ = new TestFanAutomation(entities.Object, logger.Object);

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("At least one fan must be configured*");
    }

    private sealed class TestFanAutomation(IFanAutomationEntities entities, ILogger logger)
        : FanAutomationBase(entities, logger)
    {
        protected override IEnumerable<IDisposable> GetToggleableAutomations() => [];
    }
}
