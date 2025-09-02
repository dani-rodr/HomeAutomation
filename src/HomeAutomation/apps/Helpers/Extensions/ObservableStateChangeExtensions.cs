namespace HomeAutomation.apps.Helpers.Extensions;

public static class ObservableStateChangeExtensions
{
    public static IObservable<(TLeft Left, TRight Right)> And<TLeft, TRight>(
        this IObservable<TLeft> left,
        IObservable<TRight> right
    ) => left.CombineLatest(right, (l, r) => (l, r));

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
