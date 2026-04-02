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
        _automations?.Dispose();

        var automations = new CompositeDisposable();

        try
        {
            foreach (var automation in GetAutomations())
            {
                automations.Add(automation);
            }

            _automations = automations;
        }
        catch
        {
            automations.Dispose();
            throw;
        }
    }
}
