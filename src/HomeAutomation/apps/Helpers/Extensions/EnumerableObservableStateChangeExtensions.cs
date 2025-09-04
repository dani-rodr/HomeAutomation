using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions;

public static class EnumerableObservableStateChangeExtensions
{
    public static IObservable<StateChange<T, TState>> OnChanges<T, TState, TAttributes>(
        this IEnumerable<Entity<T, TState, TAttributes>> entities,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class => Observable.Merge(entities.Select(e => e.OnChanges(options)));

    public static IObservable<StateChange<T, TState>> OnTurnedOn<T, TState, TAttributes>(
        this IEnumerable<Entity<T, TState, TAttributes>> entities,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class => Observable.Merge(entities.Select(e => e.OnTurnedOn(options)));

    public static IObservable<StateChange<T, TState>> OnTurnedOff<T, TState, TAttributes>(
        this IEnumerable<Entity<T, TState, TAttributes>> entities,
        DurationOptions<TState>? options = null
    )
        where T : Entity<T, TState, TAttributes>
        where TState : EntityState<TAttributes>
        where TAttributes : class => Observable.Merge(entities.Select(e => e.OnTurnedOff(options)));
}
