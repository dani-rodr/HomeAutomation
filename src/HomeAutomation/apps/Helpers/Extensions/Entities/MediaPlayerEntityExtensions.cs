using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class MediaPlayerEntityExtensions
{
    public static bool IsPlaying([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity?.State is HaEntityStates.PLAYING;

    public static bool IsPaused([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity?.State is HaEntityStates.PAUSED;

    public static bool IsIdle([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity?.State is HaEntityStates.IDLE;

    public static bool IsOff([NotNullWhen(true)] this MediaPlayerEntity? entity) =>
        entity?.State is HaEntityStates.OFF;
}