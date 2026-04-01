namespace HomeAutomation.apps.Common.Base;

using HomeAutomation.apps.Common.Config;

[NetDaemonApp]
public abstract class AppBase<TArea, TSettings> : IAreaApp, IDisposable
    where TArea : class
    where TSettings : class
{
    private readonly List<IAutomation> _automations = [];

    protected AppBase(IAreaConfigStore areaConfigStore)
    {
        ArgumentNullException.ThrowIfNull(areaConfigStore);

        AreaKey = ResolveAreaKey();
        Settings = ResolveSettings(areaConfigStore);

        _automations.AddRange(CreateAutomations());

        foreach (var automation in _automations)
        {
            automation.StartAutomation();
        }
    }

    public string AreaKey { get; }

    protected TSettings Settings { get; }

    protected abstract IEnumerable<IAutomation> CreateAutomations();

    private TSettings ResolveSettings(IAreaConfigStore areaConfigStore)
    {
        if (typeof(TSettings) == typeof(NoAppSettings))
        {
            return (TSettings)(object)NoAppSettings.Instance;
        }

        return areaConfigStore.GetConfig<TSettings>(AreaKey);
    }

    private static string ResolveAreaKey()
    {
        if (
            Attribute.GetCustomAttribute(typeof(TArea), typeof(AreaKeyAttribute), inherit: true)
            is not AreaKeyAttribute attribute
        )
        {
            throw new InvalidOperationException(
                $"Type '{typeof(TArea).FullName}' must be decorated with [{nameof(AreaKeyAttribute)}(\"<area-key>\")] to be used by {nameof(AppBase<TArea, TSettings>)}."
            );
        }

        if (string.IsNullOrWhiteSpace(attribute.AreaKey))
        {
            throw new InvalidOperationException(
                $"Type '{typeof(TArea).FullName}' has an invalid [{nameof(AreaKeyAttribute)}] value. The area key must be a non-empty string."
            );
        }

        return attribute.AreaKey;
    }

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
