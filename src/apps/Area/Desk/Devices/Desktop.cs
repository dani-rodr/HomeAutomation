namespace HomeAutomation.apps.Area.Desk.Devices;

public class Desktop(Entities entities, ILogger logger) : ComputerBase(logger)
{
    private readonly BinarySensorEntity powerPlugThreshold = entities.BinarySensor.SmartPlug1PowerExceedsThreshold;
    private readonly BinarySensorEntity networkStatus = entities.BinarySensor.DanielPcNetworkStatus;

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

    public override void Show()
    {
        throw new NotImplementedException();
    }

    public override void TurnOff()
    {
        throw new NotImplementedException();
    }

    public override void TurnOn()
    {
        throw new NotImplementedException();
    }
}
