using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public record DurationOptions<TState>(
    bool StartImmediately = true,
    bool AllowFromUnavailable = true,
    int Days = 0,
    int Hours = 0,
    int Minutes = 0,
    int Seconds = 0,
    int Milliseconds = 0,
    Func<TState?, bool>? Condition = null
)
{
    public TimeSpan TimeSpan => new(Days, Hours, Minutes, Seconds, Milliseconds);
    public Func<TState?, bool> SafeCondition => Condition ?? (_ => true);
}

public static class EntityExtensions
{
    public static string? StateInvariant(this Entity? entity) => entity?.State?.ToLowerInvariant();

    public static bool Is(this Entity? entity, string state) =>
        string.Equals(entity?.State, state, StringComparison.OrdinalIgnoreCase);

    public static bool IsAvailable([NotNullWhen(true)] this Entity? entity) =>
        entity.Is(HaEntityStates.UNAVAILABLE) is not true;

    public static bool IsUnavailable([NotNullWhen(true)] this Entity? entity) =>
        entity.Is(HaEntityStates.UNAVAILABLE);

    public static bool IsUnknown([NotNullWhen(true)] this Entity? entity) =>
        entity.Is(HaEntityStates.UNKNOWN);

    public static bool IsAvailable(this EntityState? state) =>
        state?.State?.ToLowerInvariant() is not HaEntityStates.UNAVAILABLE;

    public static bool IsUnavailable(this EntityState? state) =>
        state?.State?.ToLowerInvariant() is HaEntityStates.UNAVAILABLE;

    public static bool IsUnknown(this EntityState? state) =>
        state?.State?.ToLowerInvariant() is HaEntityStates.UNKNOWN;

    public static bool IsSystemOperated(this Entity? entity) =>
        HaIdentity.IsSystemOperated(entity?.EntityState?.Context?.UserId);

    public static bool IsPhysicallyOperated(this Entity? entity) =>
        HaIdentity.IsPhysicallyOperated(entity?.EntityState?.Context?.UserId);

    public static bool IsManuallyOperated(this Entity? entity) =>
        HaIdentity.IsManuallyOperated(entity?.EntityState?.Context?.UserId);
}
