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

        foreach (var automation in _automations)
        {
            automation.StartAutomation();
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
