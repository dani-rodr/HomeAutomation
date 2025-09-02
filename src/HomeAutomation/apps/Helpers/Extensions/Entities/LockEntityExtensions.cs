using System.Diagnostics.CodeAnalysis;

namespace HomeAutomation.apps.Helpers.Extensions.Entities;

public static class LockEntityExtensions
{
    public static bool IsLocked([NotNullWhen(true)] this LockEntity entity) =>
        entity.StateInvariant() is HaEntityStates.LOCKED;

    public static bool IsUnlocked([NotNullWhen(true)] this LockEntity entity) =>
        entity.StateInvariant() is HaEntityStates.UNLOCKED;

    public static bool IsLocked(this EntityState? state) =>
        state?.State?.ToLowerInvariant() == HaEntityStates.LOCKED;

    public static bool IsUnlocked(this EntityState? state) =>
        state?.State?.ToLowerInvariant() == HaEntityStates.UNLOCKED;

    public static IObservable<StateChange<T, TState>> OnLocked<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with { Condition = s => s.IsLocked() };

        return entity.OnChanges(options);
    }

    public static IObservable<StateChange<T, TState>> OnUnlocked<T, TState, TAttributes>(
        this Entity<T, TState, TAttributes> entity,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class
    {
        options = (options ?? new DurationOptions<TState>()) with
        {
            Condition = s => s.IsUnlocked(),
        };

        return entity.OnChanges(options);
    }
}
