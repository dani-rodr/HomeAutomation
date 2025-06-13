using System.Linq;

namespace HomeAutomation.apps.Common.Base;

public abstract class MediaPlayerBase : IMediaPlayer
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

    protected MediaPlayerEntity Entity { get; }
    protected ILogger Logger { get; }
    protected readonly Dictionary<string, string> Sources;

    public MediaPlayerBase(MediaPlayerEntity entity, ILogger logger)
    {
        Entity = entity;
        Logger = logger;
        Sources = SourceList?.ToDictionary(s => s, s => s) ?? [];
        ExtendSourceDictionary(Sources);
    }

    public void SetVolume(double volumeLevel) => Entity.VolumeSet(volumeLevel);

    public void Mute() => Entity.VolumeMute(true);

    public void Unmute() => Entity.VolumeMute(false);

    public void SelectSource(string source) => Entity.SelectSource(source);

    public virtual void TurnOn() => Entity.TurnOn();

    public virtual void TurnOff() => Entity.TurnOff();

    public bool IsOn() => Entity.State.IsOn();

    public bool IsOff() => Entity.State.IsOff();

    protected abstract void ExtendSourceDictionary(Dictionary<string, string> sources);

    protected void ShowSource(string key)
    {
        if (!Sources.TryGetValue(key, out var source))
        {
            Logger.LogError("Source key '{Key}' not defined.", key);
            return;
        }

        if (IsOff())
        {
            TurnOn();
        }

        SelectSource(source);
    }
}
