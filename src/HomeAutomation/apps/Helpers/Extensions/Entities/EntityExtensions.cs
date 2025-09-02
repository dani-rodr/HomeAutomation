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

    private static IObservable<StateChange<T, TState>> GetStateChange<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        bool shouldCheckImmediately = false
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        return shouldCheckImmediately ? entity.StateChangesWithCurrent() : entity.StateChanges();
    }

    public static IObservable<StateChange> IsAutomated(this IObservable<StateChange> stream) =>
        stream.Where(s => HaIdentity.IsAutomated(s.UserId()));

    public static IObservable<StateChange> IsPhysicallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsPhysicallyOperated(s.UserId()));

    public static IObservable<StateChange> IsManuallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsManuallyOperated(s.UserId()));

    private static IObservable<StateChange<T, TState>> WhenIsFor<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        TimeSpan duration
    )
        where T : Entity
        where TState : EntityState =>
        duration > TimeSpan.Zero
            ? source.WhenStateIsFor(predicate, duration, SchedulerProvider.Current)
            : source.Where(sc => predicate(sc.New));

    public static IObservable<StateChange<T, TState>> OnChanges<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options ??= new DurationOptions<TState>();

        return entity
            .GetStateChange(options.ShouldCheckImmediately)
            .WhenIsFor(options.SafeCondition, options.TimeSpan);
    }

    public static IObservable<StateChange<T, TState>> OnTurnedOn<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with { Condition = s => s.IsOn() };
        return entity.OnChanges(options);
    }

    public static IObservable<StateChange<T, TState>> OnTurnedOff<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with { Condition = s => s.IsOff() };
        return entity.OnChanges(options);
    }

    public static IObservable<StateChange<T, TState>> OnUnavailable<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with
        {
            Condition = s => s.IsUnavailable(),
        };
        return entity.OnChanges(options);
    }

    public static IObservable<StateChange<T, TState>> OnUnknown<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with
        {
            Condition = s => s.IsUnknown(),
        };

        return entity.OnChanges(options);
    }
}
