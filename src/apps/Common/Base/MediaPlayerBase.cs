using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class MediaPlayerBase(MediaPlayerEntity entity, ILogger logger)
    : AutomationBase(logger),
        IMediaPlayer
{
    public string EntityId => Entity.EntityId;
    public double? VolumeLevel => Entity.Attributes?.VolumeLevel;
    public bool? IsMuted => Entity.Attributes?.IsVolumeMuted;
    public string? CurrentSource => Entity.Attributes?.Source;
    public IReadOnlyList<string>? SourceList => Entity.Attributes?.SourceList;
    public string? FriendlyName => Entity.Attributes?.FriendlyName;
    public string? DeviceClass => Entity.Attributes?.DeviceClass;
    public string? AppId => Entity.Attributes?.AppId;
    public string? AppName => Entity.Attributes?.AppName;
    public string? MediaContentType => Entity.Attributes?.MediaContentType;
    protected abstract Dictionary<string, string> ExtendedSources { get; }
    protected MediaPlayerEntity Entity => entity;
    protected Dictionary<string, string> Sources { get; private set; } = [];
    private string _queuedSourceKey = string.Empty;

    public override void StartAutomation()
    {
        base.StartAutomation();
        Sources = SourceList?.ToDictionary(s => s, s => s) ?? [];
        ExtendedSources.ToList().ForEach(pair => Sources.Add(pair.Key, pair.Value));
    }

    protected override IEnumerable<IDisposable> GetAutomations() => [ShowQueuedSource()];

    public void SetVolume(double volumeLevel) => Entity.VolumeSet(volumeLevel);

    public void Mute() => Entity.VolumeMute(true);

    public void Unmute() => Entity.VolumeMute(false);

    public void SelectSource(string source) => Entity.SelectSource(source);

    public virtual void TurnOn() => Entity.TurnOn();

    public virtual void TurnOff() => Entity.TurnOff();

    public bool IsOn() => Entity.State.IsOn();

    public bool IsOff() => Entity.State.IsOff();

    protected void ShowSource(string key)
    {
        if (!Sources.TryGetValue(key, out var source))
        {
            Logger.LogError("Source key '{Key}' not defined.", key);
            return;
        }

        if (IsOff())
        {
            Logger.LogInformation("MediaPlayer is off. Queuing source '{Key}'.", key);
            TurnOn();
            _queuedSourceKey = key;
            return;
        }

        _queuedSourceKey = string.Empty;
        if (source != CurrentSource)
        {
            Logger.LogInformation("Switching source to '{Source}' (key: '{Key}').", source, key);
            Entity.SelectSource(source);
            return;
        }
        Logger.LogInformation(
            "Source '{Source}' (key: '{Key}') is already active. No change.",
            source,
            key
        );
    }

    public IObservable<string?> OnSourceChange() =>
        Entity
            .StateAllChangesWithCurrent()
            .Where(e => e.Old?.Attributes?.Source != e.New?.Attributes?.Source)
            .Select(e => e.New?.Attributes?.Source);

    private IDisposable ShowQueuedSource()
    {
        return Entity
            .StateChanges()
            .IsOn()
            .Subscribe(_ =>
            {
                if (string.IsNullOrEmpty(_queuedSourceKey))
                {
                    Logger.LogDebug("MediaPlayer turned on, but no queued source to apply.");
                    return;
                }
                Logger.LogInformation(
                    "MediaPlayer turned on. Applying queued source '{QueuedSource}'.",
                    _queuedSourceKey
                );
                ShowSource(_queuedSourceKey);
                _queuedSourceKey = string.Empty;
            });
    }
}
