namespace HomeAutomation.apps.Common.Base;

[NetDaemonApp]
public abstract class AreaBase<TArea> : IDisposable
    where TArea : class
{
    protected readonly Entities Entities;
    protected readonly ILogger Logger;
    protected readonly IScheduler? Scheduler;
    private readonly List<IAutomation> _automations = [];

    protected AreaBase(Entities entities, ILogger logger, IScheduler? scheduler = null)
    {
        Entities = entities;
        Logger = logger;
        Scheduler = scheduler;
        _automations.AddRange(CreateAutomations());

        foreach (var automation in _automations)
        {
            automation.StartAutomation();
        }
    }

    protected abstract IEnumerable<IAutomation> CreateAutomations();

    public virtual void Dispose()
    {
        foreach (var automation in _automations)
        {
            if (automation is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _automations.Clear();
        GC.SuppressFinalize(this);
    }
}
