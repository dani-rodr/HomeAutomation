using System.Linq;

namespace HomeAutomation.apps.Helpers;

public static class SensorEntityExtensions
{
    /// <summary>
    /// Extracts the local hour from a datetime sensor entity's state.
    /// </summary>
    /// <param name="sensor">The sensor entity containing datetime information.</param>
    /// <returns>The hour component (0-23) of the datetime, or -1 if the state is invalid or unavailable.</returns>
    /// <remarks>
    /// This method attempts to parse the sensor's state as a DateTime and extract the hour component.
    /// Returns -1 as a fallback for invalid or unparseable datetime states.
    /// </remarks>
    public static int LocalHour(this SensorEntity sensor)
    {
        if (
            sensor?.EntityState?.State is not string stateString
            || !DateTime.TryParse(stateString, out var utcTime)
        )
        {
            return -1; // Use as fallback for invalid state
        }

        return utcTime.Hour;
    }

    public static bool IsLocked(this SensorEntity sensor) => sensor.State.IsLocked();

    public static bool IsUnlocked(this SensorEntity sensor) => sensor.State.IsUnlocked();

    public static bool IsUnavailable(this SensorEntity sensor) => sensor.State.IsUnavailable();
}

public static class BinaryEntityExtensions
{
    public static bool IsOpen(this BinarySensorEntity sensor) => sensor.State.IsOpen();

    public static bool IsOccupied(this BinarySensorEntity sensor) => sensor.State.IsOn();

    public static bool IsClear(this BinarySensorEntity sensor) => sensor.State.IsOff();

    public static bool IsClosed(this BinarySensorEntity sensor) => sensor.State.IsClosed();

    public static bool IsConnected(this BinarySensorEntity sensor) => sensor.State.IsConnected();

    public static bool IsDisconnected(this BinarySensorEntity sensor) =>
        sensor.State.IsDisconnected();
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

    public static bool IsSunny(this WeatherEntity climate) =>
        climate.State is HaEntityStates.SUNNY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsRainy(this WeatherEntity climate) =>
        climate.State
            is HaEntityStates.RAINY
                or HaEntityStates.POURING
                or HaEntityStates.LIGHTNING_RAINY;

    public static bool IsCloudy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.CLOUDY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsClearNight(this WeatherEntity climate) =>
        string.Equals(
            climate.State,
            HaEntityStates.CLEAR_NIGHT,
            StringComparison.OrdinalIgnoreCase
        );

    public static bool IsStormy(this WeatherEntity climate) =>
        climate.State
            is HaEntityStates.LIGHTNING
                or HaEntityStates.LIGHTNING_RAINY
                or HaEntityStates.HAIL;

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

public static class LockEntityExtensions
{
    public static bool IsLocked(this LockEntity entity) => entity.State.IsLocked();

    public static bool IsUnlocked(this LockEntity entity) => entity.State.IsUnlocked();
}

public static class SwitchEntityExtensions
{
    /// <summary>
    /// Creates an observable that detects double-click patterns on switch entities.
    /// </summary>
    /// <param name="source">The source observable of switch state changes.</param>
    /// <param name="timeout">The maximum time in seconds between clicks to be considered a double-click.</param>
    /// <returns>An observable that emits when a double-click pattern is detected.</returns>
    /// <remarks>
    /// This method uses a sliding window approach to detect two consecutive state changes
    /// within the specified timeout period. Useful for implementing double-tap switch automations.
    /// </remarks>
    public static IObservable<
        IList<StateChange<SwitchEntity, EntityState<SwitchAttributes>>>
    > OnDoubleClick(
        this IObservable<StateChange<SwitchEntity, EntityState<SwitchAttributes>>> source,
        int timeout,
        IScheduler? scheduler = null
    )
    {
        const int maxBufferSize = 2;

        return source
            .Timestamp(scheduler ?? Scheduler.Default) // inject scheduler here
            .Buffer(maxBufferSize, 1) // sliding window of 2 consecutive changes
            .Where(pair =>
                pair.Count == maxBufferSize
                && (pair[1].Timestamp - pair[0].Timestamp) <= TimeSpan.FromSeconds(timeout)
            )
            .Select(pair => pair.Select(x => x.Value).ToList());
    }
}

public static class PersonEntityExtensions
{
    public static bool IsHome(this PersonEntity person) => person.State.IsHome();

    public static bool IsAway(this PersonEntity person) => person.State.IsAway();
}
