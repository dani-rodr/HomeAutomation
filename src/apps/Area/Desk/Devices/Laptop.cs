using HomeAutomation.apps.Common;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop(Entities entities, HaEventHandler eventHandler, ILogger logger) : ComputerBase(logger)
{
    public override bool IsOn() => false;

    public override IObservable<bool> OnHideRequested()
    {
        throw new NotImplementedException();
    }

    public override IObservable<bool> OnShowRequested()
    {
        throw new NotImplementedException();
    }

    public override IObservable<bool> StateChanges()
    {
        throw new NotImplementedException();
    }

    public override void TurnOff() { }

    public override void TurnOn() { }
}
