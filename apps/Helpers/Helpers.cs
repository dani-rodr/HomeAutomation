using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace HomeAutomation.apps.Helpers;

public static class StateChangeObservableExtensions
{
    public static IObservable<StateChange> IsState(this IObservable<StateChange> source, params string[] states)
    {
        return source.Where(e =>
            e.New?.State != null && states.Any(s => s.Equals(e.New.State, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static IObservable<StateChange> IsOn(this IObservable<StateChange> source) =>
        source.IsState(HaEntityStates.ON);

    public static IObservable<StateChange> IsOff(this IObservable<StateChange> source) =>
        source.IsState(HaEntityStates.OFF);

    public static IObservable<StateChange> IsUnavailable(this IObservable<StateChange> source) =>
        source.IsState(HaEntityStates.UNAVAILABLE);

    public static IObservable<StateChange> IsUnknown(this IObservable<StateChange> source) =>
        source.IsState(HaEntityStates.UNKNOWN);

    public static IObservable<StateChange> WhenStateIsForSeconds(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromSeconds(time), Scheduler.Default);

    public static IObservable<StateChange> WhenStateIsForMinutes(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromMinutes(time), Scheduler.Default);

    public static IObservable<StateChange> WhenStateIsForHours(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromHours(time), Scheduler.Default);

    public static IObservable<StateChange<T, TState>> WhenStateIsForSeconds<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, TimeSpan.FromSeconds(time), Scheduler.Default);
    }

    public static IObservable<StateChange<T, TState>> WhenStateIsForMinutes<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, TimeSpan.FromMinutes(time), Scheduler.Default);
    }

    public static IObservable<StateChange<T, TState>> WhenStateIsForHours<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, TimeSpan.FromHours(time), Scheduler.Default);
    }
}

public static class StateExtensions
{
    public static string UserId<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New?.Context?.UserId ?? string.Empty;
    }

    public static string State<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New?.State ?? string.Empty;
    }

    public static string UserId(this StateChange e)
    {
        return e.New?.Context?.UserId ?? string.Empty;
    }

    public static bool IsOpen(this string? state) => state.IsOn();

    public static bool IsClosed(this string? state) => state.IsOff();

    public static bool IsOn(this string? state) =>
        string.Equals(state, HaEntityStates.ON, StringComparison.OrdinalIgnoreCase);

    public static bool IsOff(this string? state) =>
        string.Equals(state, HaEntityStates.OFF, StringComparison.OrdinalIgnoreCase);

    public static bool IsUnavailable(this string? state) =>
        string.Equals(state, HaEntityStates.UNAVAILABLE, StringComparison.OrdinalIgnoreCase);
}

public static class BinaryEntityExtensions
{
    public static bool IsOpen(this BinarySensorEntity sensor) => sensor.State.IsOpen();

    public static bool IsClosed(this BinarySensorEntity sensor) => sensor.State.IsClosed();
}

public static class ClimateEntityExtensions
{
    public static bool IsDry(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.DRY, StringComparison.OrdinalIgnoreCase);

    public static bool IsCool(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.COOL, StringComparison.OrdinalIgnoreCase);

    public static bool IsOff(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.OFF, StringComparison.OrdinalIgnoreCase);

    public static bool IsOn(this ClimateEntity climate) => climate.IsDry() || climate.IsCool();
}

public static class NumberEntityExtensions
{
    public static void SetNumericValue(this NumberEntity entity, double value)
    {
        entity.CallService("set_value", new { value });
    }
}

public static class SwitchEntityExtensions
{
    public static IObservable<IList<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>> OnDoubleClick(
        this IObservable<StateChange<SwitchEntity, EntityState<SwitchAttributes>>> source,
        int timeout
    )
    {
        var maxBufferSize = 2;
        return source
            .Timestamp() // adds timestamp to each state change
            .Buffer(maxBufferSize, 1) // sliding window of 2 consecutive changes
            .Where(pair =>
                pair.Count == maxBufferSize && (pair[1].Timestamp - pair[0].Timestamp) <= TimeSpan.FromSeconds(timeout)
            )
            .Select(pair => pair.Select(x => x.Value).ToList());
    }
}

public static class TimeRange
{
    public static bool IsCurrentTimeInBetween(int start, int end) =>
        IsTimeInBewteen(DateTime.Now.TimeOfDay, start, end);

    public static bool IsTimeInBewteen(TimeSpan now, int start, int end)
    {
        var startTime = TimeSpan.FromHours(start);
        var endTime = TimeSpan.FromHours(end);
        return start <= end ? now >= startTime && now <= endTime : now >= startTime || now <= endTime;
    }
}
