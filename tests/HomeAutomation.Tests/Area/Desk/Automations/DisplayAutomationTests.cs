using HomeAutomation.apps.Area.Desk.Automations;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.Tests.Area.Desk.Automations;

public class DisplayAutomationTests : HaContextTestBase
{
    private readonly Mock<ILgDisplay> _monitor = new();
    private readonly Mock<IComputer> _desktop = new();
    private readonly Mock<ILogger<DisplayAutomation>> _logger = new();

    private readonly Subject<bool> _desktopStateChanges = new();
    private readonly Subject<bool> _showRequests = new();
    private readonly Subject<bool> _hideRequests = new();

    private readonly DisplayAutomation _automation;
    private bool _desktopIsOn;

    public DisplayAutomationTests()
    {
        _desktop.Setup(x => x.StateChanges()).Returns(_desktopStateChanges);
        _desktop.Setup(x => x.OnShowRequested()).Returns(_showRequests);
        _desktop.Setup(x => x.OnHideRequested()).Returns(_hideRequests);
        _desktop.Setup(x => x.IsOn()).Returns(() => _desktopIsOn);

        var masterSwitch = new SwitchEntity(HaContext, "switch.master_switch");
        HaContext.SetEntityState(masterSwitch.EntityId, HaEntityStates.ON);

        _automation = new DisplayAutomation(
            _monitor.Object,
            _desktop.Object,
            masterSwitch,
            _logger.Object
        );
        _automation.StartAutomation();
    }

    [Fact]
    public void DesktopStateChange_WhenTurnsOn_Should_ShowPcOnMonitor()
    {
        EmitDesktopState(true);

        _monitor.Verify(x => x.ShowPC(), Times.Once);
    }

    [Fact]
    public void DesktopStateChange_WhenTurnsOff_Should_TurnOffMonitor()
    {
        EmitDesktopState(true);
        _monitor.Invocations.Clear();

        EmitDesktopState(false);

        _monitor.Verify(x => x.TurnOff(), Times.Once);
    }

    [Fact]
    public void ShowPcEvent_Should_ShowPcOnMonitor()
    {
        _showRequests.OnNext(true);

        _monitor.Verify(x => x.ShowPC(), Times.Once);
    }

    [Fact]
    public void HidePcEvent_Should_TurnOffMonitor_AndDesktop()
    {
        _hideRequests.OnNext(true);

        _monitor.Verify(x => x.TurnOff(), Times.Once);
        _desktop.Verify(x => x.TurnOff(), Times.Once);
    }

    [Fact]
    public void Automation_WhenDisposed_Should_NotReactToDesktopChanges()
    {
        _automation.Dispose();
        _monitor.Invocations.Clear();
        _desktop.Invocations.Clear();

        EmitDesktopState(true);
        _showRequests.OnNext(true);
        _hideRequests.OnNext(true);

        _monitor.Verify(x => x.ShowPC(), Times.Never);
        _monitor.Verify(x => x.TurnOff(), Times.Never);
        _desktop.Verify(x => x.TurnOff(), Times.Never);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _automation.Dispose();
            _desktopStateChanges.Dispose();
            _showRequests.Dispose();
            _hideRequests.Dispose();
        }

        base.Dispose(disposing);
    }

    private void EmitDesktopState(bool isOn)
    {
        _desktopIsOn = isOn;
        _desktopStateChanges.OnNext(isOn);
    }
}
