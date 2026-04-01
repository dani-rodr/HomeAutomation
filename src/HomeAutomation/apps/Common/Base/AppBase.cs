namespace HomeAutomation.apps.Common.Base;

[NetDaemonApp]
public abstract class AppBase<TArea, TSettings> : IAreaApp, IDisposable
    where TArea : class
    where TSettings : class
{
    private readonly List<IAutomation> _automations = [];

    protected AppBase(TSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        AreaKey = ResolveAreaKey();
        Settings = settings;

        _automations.AddRange(CreateAutomations());

        foreach (var automation in _automations)
        {
            automation.StartAutomation();
        }
    }

    public string AreaKey { get; }

    protected TSettings Settings { get; }

    protected abstract IEnumerable<IAutomation> CreateAutomations();

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
