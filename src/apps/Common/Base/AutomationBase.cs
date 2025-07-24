using System.Reactive.Disposables;

namespace HomeAutomation.apps.Common.Base;

public abstract class AutomationBase(ILogger logger) : IAutomation
{
    protected abstract IEnumerable<IDisposable> GetAutomations();
    private CompositeDisposable? _automations;
    protected readonly ILogger Logger = logger;

    public virtual void Dispose()
    {
        _automations?.Dispose();
        _automations = null;
        GC.SuppressFinalize(this);
    }

    public virtual void StartAutomation()
    {
        _automations = [.. GetAutomations()];
    }
}
