using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public record DurationOptions<TState>(
    bool ShouldCheckImmediately = false,
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

    public static bool IsUnavailable([NotNullWhen(true)] this Entity? entity) =>
        entity.StateInvariant() is HaEntityStates.UNAVAILABLE;

    public static bool IsUnknown([NotNullWhen(true)] this Entity? entity) =>
        entity.StateInvariant() is HaEntityStates.UNKNOWN;

    public static bool IsUnavailable(this EntityState? state) =>
        state?.State?.ToLowerInvariant() == HaEntityStates.UNAVAILABLE;

    public static bool IsUnknown(this EntityState? state) =>
        state?.State?.ToLowerInvariant() == HaEntityStates.UNKNOWN;
}
