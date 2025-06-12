using HomeAutomation.apps.Common;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(Entities entities, HaEventHandler eventHandler, ILogger logger) : ComputerBase(logger)
{
    private const string SHOW_PC_EVENT = "show_pc";
    private const string HIDE_PC_EVENT = "hide_pc";
    private readonly BinarySensorEntity powerPlugThreshold = entities.BinarySensor.SmartPlug1PowerExceedsThreshold;
    private readonly BinarySensorEntity networkStatus = entities.BinarySensor.DanielPcNetworkStatus;
    private readonly SwitchEntity powerSwitch = entities.Switch.WakeOnLan;

    public IObservable<bool> GetPowerState()
    {
        return Observable
            .CombineLatest(
                powerPlugThreshold.StateChanges().Select(s => s.New?.State),
                networkStatus.StateChanges().Select(s => s.New?.State),
                EvaluatePowerState
            )
            .StartWith(EvaluatePowerState(powerPlugThreshold.State, networkStatus.State));
    }

    private static bool EvaluatePowerState(string? powerState, string? netState)
    {
        if (netState.IsDisconnected())
        {
            return false;
        }
        return powerState.IsOn() || netState.IsConnected();
    }

    public IObservable<bool> OnShowRequested() => eventHandler.WhenEventTriggered(SHOW_PC_EVENT).Select(_ => true);

    public IObservable<bool> OnHideRequested() => eventHandler.WhenEventTriggered(HIDE_PC_EVENT).Select(_ => true);

    public override void TurnOff() => powerSwitch.TurnOff();

    public override void TurnOn() => powerSwitch.TurnOn();
}
