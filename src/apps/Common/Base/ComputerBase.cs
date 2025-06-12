using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common.Base;

public abstract class ComputerBase : IComputer
{
    public abstract void TurnOn();
    public abstract void TurnOff();
    public abstract void Show();
}
