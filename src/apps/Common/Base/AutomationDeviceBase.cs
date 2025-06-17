using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class AutomationDeviceBase : IAutomationDevice
{
    protected CompositeDisposable Automations = [];

    public virtual void Dispose()
    {
        Automations.Dispose();
        GC.SuppressFinalize(this);
    }
}
