using System.Linq;

namespace HomeAutomation.apps.Helpers;

public enum TimeUnit
{
    Seconds,
    Minutes,
    Hours,
}

public static class StateChangeObservableExtensions
{
    public static IObservable<StateChange> IsAnyOfStates(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true,
        params string[] states
    ) =>
        source.Where(e =>
            (e.New?.State.IsAvailable() ?? false)
            && (
                ignorePreviousUnavailable
                || (
                    e.Old?.State != null && !e.Old.State.IsUnavailable() && !e.Old.State.IsUnknown()
                )
            )
            && states.Any(s => string.Equals(s, e.New.State, StringComparison.OrdinalIgnoreCase))
        );

    public static IObservable<StateChange> WasOff(this IObservable<StateChange> source) =>
        source.Where(e => e.Old.IsOff());

    public static IObservable<StateChange> WasOn(this IObservable<StateChange> source) =>
        source.Where(e => e.Old.IsOn());

    public static IObservable<StateChange> IsOn(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.ON);

    public static IObservable<StateChange> IsOpen(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsOn(ignorePreviousUnavailable);

    public static IObservable<StateChange> IsOff(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.OFF);

    public static IObservable<StateChange> IsClosed(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsOff(ignorePreviousUnavailable);

    public static IObservable<StateChange> IsLocked(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.LOCKED);

    public static IObservable<StateChange> IsUnlocked(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.UNLOCKED);

    public static IObservable<StateChange> IsUnavailable(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) =>
        source.Where(s =>
            string.Equals(
                s.New?.State,
                HaEntityStates.UNAVAILABLE,
                StringComparison.OrdinalIgnoreCase
            )
            && (
                !ignorePreviousUnavailable || (s.Old?.State != null && !s.Old.State.IsUnavailable())
            )
        );

    public static IObservable<StateChange> IsUnknown(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) =>
        source.Where(s =>
            string.Equals(s.New?.State, HaEntityStates.UNKNOWN, StringComparison.OrdinalIgnoreCase)
            && (
                !ignorePreviousUnavailable || (s.Old?.State != null && !s.Old.State.IsUnavailable())
            )
        );

    public static IObservable<StateChange> IsManuallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsManuallyOperated(s.UserId()));

    public static IObservable<StateChange> IsPhysicallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsPhysicallyOperated(s.UserId()));

    public static IObservable<StateChange> IsAutomated(this IObservable<StateChange> source) =>
        source.Where(s => HaIdentity.IsAutomated(s.UserId()));

    public static IObservable<StateChange> IsValidButtonPress(
        this IObservable<StateChange> source
    ) => source.Where(s => s.IsValidButtonPress());

    public static IObservable<IList<StateChange>> IsFlickering(
        this IObservable<StateChange> source,
        int minimumFlips = 4,
        int timeWindowMs = 10000,
        IScheduler? scheduler = null
    ) =>
        source
            .Where(e => e.New?.State != null)
            .DistinctUntilChanged(e => e.New?.State)
            .Buffer(TimeSpan.FromMilliseconds(timeWindowMs), scheduler ?? SchedulerProvider.Current)
            .Where(events => events.Count >= minimumFlips);
}

public static class StateChangeDurationExtensions
{
    public static IObservable<StateChange> ForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.For(time, TimeUnit.Seconds);

    public static IObservable<StateChange> ForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.For(time, TimeUnit.Minutes);

    public static IObservable<StateChange> ForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.For(time, TimeUnit.Hours);

    private static IObservable<StateChange> For(
        this IObservable<StateChange> source,
        int time,
        TimeUnit unit
    )
    {
        var timeSpan = unit switch
        {
            TimeUnit.Seconds => TimeSpan.FromSeconds(time),
            TimeUnit.Minutes => TimeSpan.FromMinutes(time),
            TimeUnit.Hours => TimeSpan.FromHours(time),
            _ => throw new ArgumentOutOfRangeException(nameof(unit)),
        };

        return source
            .Select(e => Observable.Timer(timeSpan, SchedulerProvider.Current).Select(_ => e))
            .Switch(); // Cancels previous timer if a new event comes
    }
}
