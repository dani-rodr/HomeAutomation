using HomeAssistantGenerated;
using HomeAutomation.apps.Common.Interface;

namespace HomeAutomation.apps.Common.Base;

public abstract class MediaPlayerBase(MediaPlayerEntity entity) : IMediaPlayer
{
    protected MediaPlayerEntity Entity { get; } = entity;

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

    public void SetVolume(double volumeLevel) => Entity.VolumeSet(volumeLevel);

    public void Mute() => Entity.VolumeMute(true);

    public void Unmute() => Entity.VolumeMute(false);

    public void SelectSource(string source) => Entity.SelectSource(source);

    public virtual void TurnOn() => Entity.TurnOn();

    public void TurnOff() => Entity.TurnOff();

    public bool IsOn() => Entity.State.IsOn();

    public bool IsOff() => Entity.State.IsOff();
}
