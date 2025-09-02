using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class MediaPlayerBase(MediaPlayerEntity entity, ILogger logger)
    : AutomationBase(logger),
        IMediaPlayer
{
    public string EntityId => MediaPlayer.EntityId;
    public double? VolumeLevel => MediaPlayer.Attributes?.VolumeLevel;
    public bool? IsMuted => MediaPlayer.Attributes?.IsVolumeMuted;
    public string? CurrentSource => MediaPlayer.Attributes?.Source;
    public IReadOnlyList<string>? SourceList => MediaPlayer.Attributes?.SourceList;
    public string? FriendlyName => MediaPlayer.Attributes?.FriendlyName;
    public string? DeviceClass => MediaPlayer.Attributes?.DeviceClass;
    public string? AppId => MediaPlayer.Attributes?.AppId;
    public string? AppName => MediaPlayer.Attributes?.AppName;
    public string? MediaContentType => MediaPlayer.Attributes?.MediaContentType;
    protected abstract Dictionary<string, string> ExtendedSources { get; }
    protected MediaPlayerEntity MediaPlayer => entity;
    protected Dictionary<string, string> Sources { get; private set; } = [];
    private string _queuedSourceKey = string.Empty;

    public override void StartAutomation()
    {
        base.StartAutomation();
        Sources = SourceList?.ToDictionary(s => s, s => s) ?? [];
        ExtendedSources.ToList().ForEach(pair => Sources.Add(pair.Key, pair.Value));
    }

    protected override IEnumerable<IDisposable> GetAutomations() => [ShowQueuedSource()];

    public void SetVolume(double volumeLevel) => MediaPlayer.VolumeSet(volumeLevel);

    public void Mute() => MediaPlayer.VolumeMute(true);

    public void Unmute() => MediaPlayer.VolumeMute(false);

    public void SelectSource(string source) => MediaPlayer.SelectSource(source);

    public virtual void TurnOn() => MediaPlayer.TurnOn();

    public virtual void TurnOff() => MediaPlayer.TurnOff();

    public bool IsOn() => MediaPlayer.State.IsOn();

    public bool IsOff() => MediaPlayer.State.IsOff();

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
            MediaPlayer.SelectSource(source);
            return;
        }
        Logger.LogInformation(
            "Source '{Source}' (key: '{Key}') is already active. No change.",
            source,
            key
        );
    }

    public IObservable<string?> OnSourceChange() =>
        MediaPlayer
            .StateAllChangesWithCurrent()
            .Where(e => e.Old?.Attributes?.Source != e.New?.Attributes?.Source)
            .Select(e => e.New?.Attributes?.Source);

    private IDisposable ShowQueuedSource()
    {
        return MediaPlayer
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
