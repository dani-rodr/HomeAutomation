using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public record DurationOptions<TState>(
    bool CheckImmediately = false,
    bool IgnoreUnavailableState = false,
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
}
