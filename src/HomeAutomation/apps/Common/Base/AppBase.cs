namespace HomeAutomation.apps.Common.Base;

[NetDaemonApp]
public abstract class AppBase<TSettings> : IDisposable
    where TSettings : class, new()
{
    private readonly List<IAutomation> _automations = [];

    protected AppBase(IAppConfig<TSettings> appConfig)
    {
        ArgumentNullException.ThrowIfNull(appConfig);
        ArgumentNullException.ThrowIfNull(appConfig.Value);

        Settings = appConfig.Value;

        _automations.AddRange(CreateAutomations());

        var startedAutomations = new List<IAutomation>();

        try
        {
            foreach (var automation in _automations)
            {
                startedAutomations.Add(automation);
                automation.StartAutomation();
            }
        }
        catch
        {
            foreach (var startedAutomation in startedAutomations)
            {
                if (startedAutomation is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            throw;
        }
    }

    protected TSettings Settings { get; }

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
