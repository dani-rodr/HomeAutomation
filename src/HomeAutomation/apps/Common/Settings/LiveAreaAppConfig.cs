using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace HomeAutomation.apps.Common.Settings;

public sealed class LiveAreaAppConfig<T> : ILiveAppConfig<T>, IDisposable
    where T : class, new()
{
    private readonly IAreaSettingsStore _areaSettingsStore;
    private readonly ILogger<LiveAreaAppConfig<T>> _logger;
    private readonly string? _areaKey;
    private readonly Subject<T> _changes = new();
    private readonly IDisposable _subscription;
    private T _value;

    public LiveAreaAppConfig(
        IAppConfig<T> appConfig,
        IAreaSettingsStore areaSettingsStore,
        IAreaSettingsChangeNotifier areaSettingsChangeNotifier,
        AreaSettingsRegistry registry,
        ILogger<LiveAreaAppConfig<T>> logger
    )
    {
        _areaSettingsStore = areaSettingsStore;
        _logger = logger;

        if (registry.TryGetBySettingsType(typeof(T), out var descriptor))
        {
            _areaKey = descriptor.Key;
            _value = _areaSettingsStore.GetSettings<T>(descriptor.Key);

            _subscription = areaSettingsChangeNotifier
                .Changes.Where(change =>
                    string.Equals(
                        change.AreaKey,
                        descriptor.Key,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Subscribe(_ => ReloadFromStore());
        }
        else
        {
            _value = appConfig.Value;
            _subscription = Disposable.Empty;
        }
    }

    public T Value => _value;

    public T Settings => _value;

    public IObservable<T> Changes => _changes;

    public void Dispose()
    {
        _subscription.Dispose();
        _changes.Dispose();
    }

    private void ReloadFromStore()
    {
        if (string.IsNullOrWhiteSpace(_areaKey))
        {
            return;
        }

        _value = _areaSettingsStore.GetSettings<T>(_areaKey);
        _changes.OnNext(_value);

        _logger.LogInformation(
            "Reloaded live settings for area '{AreaKey}' and type '{SettingsTypeName}'.",
            _areaKey,
            typeof(T).Name
        );
    }
}
