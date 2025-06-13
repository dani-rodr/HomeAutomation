using HomeAutomation.apps.Common;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Area.Desk.Devices;

public class Laptop(IEventHandler eventHandler, ILogger logger) : ComputerBase(logger)
{
    public override bool IsOn() => true;

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
