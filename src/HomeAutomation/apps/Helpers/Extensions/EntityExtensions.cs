using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HomeAutomation.apps.Helpers.Extensions;

public static class EntityExtensions
{
    public static bool IsUnavailable([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.UNAVAILABLE;

    public static bool IsUnknown([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.UNAVAILABLE;
}

public static class SensorEntityExtensions
{
    public static bool IsConnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.CONNECTED;

    public static bool IsDisconnected([NotNullWhen(true)] this SensorEntity? entity) =>
        entity?.State is HaEntityStates.DISCONNECTED;

    public static int ToLocalHour(this SensorEntity sensor)
    {
        if (sensor.State is not string state || !DateTime.TryParse(state, out var utcTime))
        {
            return -1; // Use as fallback for invalid state
        }

        return utcTime.Hour;
    }
}

public static class BinaryEntityExtensions
{
    public static bool IsOpen([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity?.State is HaEntityStates.ON;

    public static bool IsClosed([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity?.State is HaEntityStates.CLOSED;

    public static bool IsOccupied([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity?.State is HaEntityStates.ON;

    public static bool IsClear([NotNullWhen(true)] this BinarySensorEntity? entity) =>
        entity?.State is HaEntityStates.OFF;
}

public static class PersonEntityExtensions
{
    public static bool IsHome([NotNullWhen(true)] this PersonEntity entity) =>
        entity?.State is HaEntityStates.HOME;

    public static bool IsAway([NotNullWhen(true)] this PersonEntity entity) =>
        entity?.State is HaEntityStates.AWAY;
}

public static class LockEntityExtensions
{
    public static bool IsLocked([NotNullWhen(true)] this LockEntity? entity) =>
        entity?.State is HaEntityStates.LOCKED;

    public static bool IsUnlocked([NotNullWhen(true)] this LockEntity? entity) =>
        entity?.State is HaEntityStates.UNLOCKED;
}

public static class ClimateEntityExtensions
{
    public static bool IsDry([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate?.State is HaEntityStates.DRY;

    public static bool IsCool([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate?.State is HaEntityStates.COOL;

    public static bool IsOn([NotNullWhen(true)] this ClimateEntity? climate) =>
        climate?.State is HaEntityStates.DRY or HaEntityStates.COOL;
}

public static class WeatherEntityExtensions
{
    public static bool IsDry([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State is HaEntityStates.DRY;

    public static bool IsSunny([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State is HaEntityStates.SUNNY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsRainy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State
            is HaEntityStates.RAINY
                or HaEntityStates.POURING
                or HaEntityStates.LIGHTNING_RAINY;

    public static bool IsCloudy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State is HaEntityStates.CLOUDY or HaEntityStates.PARTLY_CLOUDY;

    public static bool IsClearNight([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State is HaEntityStates.CLEAR_NIGHT;

    public static bool IsStormy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State
            is HaEntityStates.LIGHTNING
                or HaEntityStates.LIGHTNING_RAINY
                or HaEntityStates.HAIL;

    public static bool IsSnowy([NotNullWhen(true)] this WeatherEntity? weather) =>
        weather?.State is HaEntityStates.SNOWY or HaEntityStates.SNOWY_RAINY;
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
