using System.Linq;

namespace HomeAutomation.apps.Helpers;

public enum TimeUnit
{
    Seconds,
    Minutes,
    Hours,
}

public readonly struct StateChangeFilter(IObservable<StateChange> source, bool useNewState)
{
    private readonly IObservable<StateChange> _source = source;
    private readonly bool _useNewState = useNewState;

    public IObservable<StateChange> State(string expectedState)
    {
        var src = _source;
        var useNew = _useNewState;

        return src.Where(e =>
        {
            var state = useNew ? e.New?.State : e.Old?.State;
            return string.Equals(state, expectedState, StringComparison.OrdinalIgnoreCase);
        });
    }

    public IObservable<StateChange> On() => State(HaEntityStates.ON);

    public IObservable<StateChange> Off() => State(HaEntityStates.OFF);

    public IObservable<StateChange> Locked() => State(HaEntityStates.LOCKED);

    public IObservable<StateChange> Unlocked() => State(HaEntityStates.UNLOCKED);
}

public static class StateChangeObservableExtensions
{
    public static StateChangeFilter Is(this IObservable<StateChange> source) =>
        new(source, useNewState: true);

    public static StateChangeFilter Was(this IObservable<StateChange> source) =>
        new(source, useNewState: false);

    public static IObservable<StateChange> WasUnlocked(this IObservable<StateChange> source) =>
        source.Was().Unlocked();

    public static IObservable<StateChange> IsOn(this IObservable<StateChange> source) =>
        source.Is().On();

    public static IObservable<StateChange> IsOff(this IObservable<StateChange> source) =>
        source.Is().Off();

    public static IObservable<StateChange> IsLocked(this IObservable<StateChange> source) =>
        source.Is().Locked();

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

    public static IObservable<StateChange> For(
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
