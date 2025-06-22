using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class AutomationDeviceBase : IAutomationDevice
{
    protected abstract CompositeDisposable Automations { get; }

    public virtual void Dispose()
    {
        Automations.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual void StartAutomation()
    {
        _ = Automations; // This is the load the values in this container
    }
}
