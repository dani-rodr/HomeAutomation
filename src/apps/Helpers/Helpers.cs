using System.Linq;

namespace HomeAutomation.apps.Helpers;

public static class StateChangeObservableExtensions
{
    public static IObservable<StateChange> IsAnyOfStates(
        this IObservable<StateChange> source,
        params string[] states
    ) =>
        source.Where(e =>
            e.New?.State != null && states.Any(s => s.Equals(e.New.State, StringComparison.OrdinalIgnoreCase))
        );

    public static IObservable<StateChange> IsOn(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.ON);

    public static IObservable<StateChange> IsOpen(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.ON);

    public static IObservable<StateChange> IsOff(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.OFF);

    public static IObservable<StateChange> IsClosed(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.OFF);

    public static IObservable<StateChange> IsUnavailable(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.UNAVAILABLE);

    public static IObservable<StateChange> IsUnknown(this IObservable<StateChange> source) =>
        source.IsAnyOfStates(HaEntityStates.UNKNOWN);

    public static IObservable<StateChange> IsManuallyOperated(this IObservable<StateChange> source) =>
        source.Where(s => HaIdentity.IsManuallyOperated(s.UserId()));

    public static IObservable<StateChange> IsPhysicallyOperated(this IObservable<StateChange> source) =>
        source.Where(s => HaIdentity.IsPhysicallyOperated(s.UserId()));

    public static IObservable<StateChange> IsAutomated(this IObservable<StateChange> source) =>
        source.Where(s => HaIdentity.IsAutomated(s.UserId()));

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

    public static IObservable<StateChange> IsOnForSeconds(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForSeconds(HaEntityStates.ON, time);

    public static IObservable<StateChange> IsOnForMinutes(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForMinutes(HaEntityStates.ON, time);

    public static IObservable<StateChange> IsOnForHours(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForHours(HaEntityStates.ON, time);

    public static IObservable<StateChange> IsOffForSeconds(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForSeconds(HaEntityStates.OFF, time);

    public static IObservable<StateChange> IsOffForMinutes(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForMinutes(HaEntityStates.OFF, time);

    public static IObservable<StateChange> IsOffForHours(this IObservable<StateChange> source, int time) =>
        source.WhenStateIsForHours(HaEntityStates.OFF, time);

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

    public static bool IsOn<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsOn();
    }

    public static bool IsOff<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsOff();
    }

    public static bool IsLocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsLocked();
    }

    public static bool IsUnlocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsUnlocked();
    }

    public static string UserId(this StateChange e)
    {
        return e.New?.Context?.UserId ?? string.Empty;
    }

    public static bool IsValidButtonPress(this StateChange e) => DateTime.TryParse(e?.New?.State, out _);

    public static bool IsOn(this StateChange e) => e?.New?.State?.IsOn() ?? false;

    public static bool IsOff(this StateChange e) => e?.New?.State?.IsOff() ?? true;

    public static bool IsOpen(this string? state) => state.IsOn();

    public static bool IsClosed(this string? state) => state.IsOff();

    public static bool IsLocked(this string? state) =>
        string.Equals(state, HaEntityStates.LOCKED, StringComparison.OrdinalIgnoreCase);

    public static bool IsUnlocked(this string? state) =>
        string.Equals(state, HaEntityStates.UNLOCKED, StringComparison.OrdinalIgnoreCase);

    public static bool IsOn(this string? state) =>
        string.Equals(state, HaEntityStates.ON, StringComparison.OrdinalIgnoreCase);

    public static bool IsOff(this string? state) =>
        string.Equals(state, HaEntityStates.OFF, StringComparison.OrdinalIgnoreCase);

    public static bool IsConnected(this string? state) =>
        string.Equals(state, HaEntityStates.CONNECTED, StringComparison.OrdinalIgnoreCase);

    public static bool IsDisconnected(this string? state) =>
        string.Equals(state, HaEntityStates.DISCONNECTED, StringComparison.OrdinalIgnoreCase);

    public static bool IsUnavailable(this string? state) =>
        string.Equals(state, HaEntityStates.UNAVAILABLE, StringComparison.OrdinalIgnoreCase);
}

public static class SensorEntityExtensions
{
    public static int LocalHour(this SensorEntity sensor)
    {
        if (sensor?.EntityState?.State is not string stateString || !DateTime.TryParse(stateString, out var utcTime))
        {
            return -1; // Use as fallback for invalid state
        }

        return utcTime.Hour;
    }

    public static bool IsLocked(this SensorEntity sensor) => sensor?.State.IsLocked() == true;

    public static bool IsUnlocked(this SensorEntity sensor) => sensor?.State.IsUnlocked() == true;
}

public static class BinaryEntityExtensions
{
    public static bool IsOpen(this BinarySensorEntity sensor) => sensor.State.IsOpen();

    public static bool IsOccupied(this BinarySensorEntity sensor) => sensor.State.IsOn();

    public static bool IsClear(this BinarySensorEntity sensor) => sensor.State.IsOff();

    public static bool IsClosed(this BinarySensorEntity sensor) => sensor.State.IsClosed();

    public static bool IsConnected(this BinarySensorEntity sensor) => sensor.State.IsConnected();

    public static bool IsDisconnected(this BinarySensorEntity sensor) => sensor.State.IsDisconnected();
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

public static class WeatherEntityExtensions
{
    public static bool IsDry(this WeatherEntity climate) =>
        string.Equals(climate.State, HaEntityStates.DRY, StringComparison.OrdinalIgnoreCase);

    public static bool IsSunny(this WeatherEntity climate)
    {
        return (
            string.Equals(climate.State, HaEntityStates.SUNNY, StringComparison.OrdinalIgnoreCase)
            || string.Equals(climate.State, HaEntityStates.PARTLY_CLOUDY, StringComparison.OrdinalIgnoreCase)
        );
    }

    public static bool IsRainy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.RAINY or HaEntityStates.POURING or HaEntityStates.LIGHTNING_RAINY;

    public static bool IsCloudy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.CLOUDY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsClearNight(this WeatherEntity climate) =>
        string.Equals(climate.State, HaEntityStates.CLEAR_NIGHT, StringComparison.OrdinalIgnoreCase);

    public static bool IsStormy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.LIGHTNING or HaEntityStates.LIGHTNING_RAINY or HaEntityStates.HAIL;

    public static bool IsSnowy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.SNOWY or HaEntityStates.SNOWY_RAINY;
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
        IsTimeInBetween(DateTime.Now.TimeOfDay, start, end);

    public static bool IsTimeInBetween(TimeSpan now, int start, int end)
    {
        var startTime = TimeSpan.FromHours(start);
        var endTime = TimeSpan.FromHours(end);
        return start <= end ? now >= startTime && now <= endTime : now >= startTime || now <= endTime;
    }
}
