using System.Linq;
using System.Reactive.Concurrency;

namespace HomeAutomation.apps.Helpers;

public static class StateChangeObservableExtensions
{
    public static IObservable<StateChange> IsState(this IObservable<StateChange> source, params string[] states)
    {
        return source.Where(e =>
            e.New?.State != null &&
            states.Any(s => s.Equals(e.New.State, StringComparison.OrdinalIgnoreCase))
        );
    }
    public static IObservable<StateChange> IsOn(this IObservable<StateChange> source) => source.IsState(HaEntityStates.ON);
    public static IObservable<StateChange> IsOff(this IObservable<StateChange> source) => source.IsState(HaEntityStates.OFF);
    public static IObservable<StateChange> IsUnavailable(this IObservable<StateChange> source) => source.IsState(HaEntityStates.UNAVAILABLE);
    public static IObservable<StateChange> IsUnknown(this IObservable<StateChange> source) => source.IsState(HaEntityStates.UNKNOWN);

    public static IObservable<StateChange> WhenStateIsForSeconds(
        this IObservable<StateChange> source,
        string desiredState,
        int seconds)
    {
        return source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromSeconds(seconds), Scheduler.Default);
    }

    public static IObservable<StateChange> WhenStateIsForMinutes(
        this IObservable<StateChange> source,
        string desiredState,
        int minutes)
    {
        return source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromMinutes(minutes), Scheduler.Default);
    }

    public static IObservable<StateChange> WhenStateIsForHours(
        this IObservable<StateChange> source,
        string desiredState,
        int hours)
    {
        return source.WhenStateIsFor(s => s?.State == desiredState, TimeSpan.FromHours(hours), Scheduler.Default);
    }

}

public static class NumberEntityExtensions
{
    public static void SetNumericValue(this NumberEntity entity, double value)
    {
        entity.CallService("set_value", new { value });
    }
}