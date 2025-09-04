using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class MediaPlayerEntityExtensions
{
    public static bool IsPlaying([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.PLAYING;

    public static bool IsPaused([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.PAUSED;

    public static bool IsIdle([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.IDLE;

    public static bool IsOff([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity.StateInvariant() is HaEntityStates.OFF;
}
