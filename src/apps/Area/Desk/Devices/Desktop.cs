using HomeAutomation.apps.Common;
using HomeAutomation.apps.Common.Containers;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(IDesktopEntities entities, IEventHandler eventHandler, ILogger logger) : ComputerBase(logger)
{
    private const string SHOW_PC_EVENT = "show_pc";
    private const string HIDE_PC_EVENT = "hide_pc";
    private readonly BinarySensorEntity powerPlugThreshold = entities.PowerPlugThreshold;
    private readonly BinarySensorEntity networkStatus = entities.NetworkStatus;
    private readonly SwitchEntity powerSwitch = entities.PowerSwitch;

    public override bool IsOn() => GetPowerState(powerPlugThreshold.State, networkStatus.State);

    public override IObservable<bool> StateChanges()
    {
        return Observable
            .CombineLatest(
                powerPlugThreshold.StateChanges().Select(s => s.New?.State),
                networkStatus.StateChanges().Select(s => s.New?.State),
                GetPowerState
            )
            .StartWith(GetPowerState(powerPlugThreshold.State, networkStatus.State));
    }

    private static bool GetPowerState(string? powerState, string? netState)
    {
        if (netState.IsDisconnected())
        {
            return false;
        }
        return powerState.IsOn() || netState.IsConnected();
    }

    public override IObservable<bool> OnShowRequested() =>
        eventHandler.WhenEventTriggered(SHOW_PC_EVENT).Select(_ => true);

    public override IObservable<bool> OnHideRequested() =>
        eventHandler.WhenEventTriggered(HIDE_PC_EVENT).Select(_ => true);

    public override void TurnOff() => powerSwitch.TurnOff();

    public override void TurnOn() => powerSwitch.TurnOn();
}
