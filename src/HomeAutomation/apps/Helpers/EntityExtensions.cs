using System.Linq;
using System.Runtime.CompilerServices;

namespace HomeAutomation.apps.Helpers;

/// <summary>
/// Extension methods for SensorEntity providing additional state checking and data extraction capabilities.
/// </summary>
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

    /// <summary>
    /// Determines if the sensor entity is in a locked state.
    /// </summary>
    /// <param name="sensor">The sensor entity to check.</param>
    /// <returns>True if the sensor state indicates locked, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLocked(this SensorEntity sensor) => sensor?.State.IsLocked() == true;

    /// <summary>
    /// Determines if the sensor entity is in an unlocked state.
    /// </summary>
    /// <param name="sensor">The sensor entity to check.</param>
    /// <returns>True if the sensor state indicates unlocked, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnlocked(this SensorEntity sensor) => sensor?.State.IsUnlocked() == true;

    /// <summary>
    /// Determines if the sensor entity is unavailable.
    /// </summary>
    /// <param name="sensor">The sensor entity to check.</param>
    /// <returns>True if the sensor is unavailable, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnavailable(this SensorEntity sensor) =>
        sensor?.State.IsUnavailable() == true;
}

/// <summary>
/// Extension methods for BinarySensorEntity providing intuitive state checking for various sensor types.
/// </summary>
public static class BinaryEntityExtensions
{
    /// <summary>
    /// Determines if the binary sensor indicates an open state (door, window, etc.).
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates open state, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpen(this BinarySensorEntity sensor) => sensor.State.IsOpen();

    /// <summary>
    /// Determines if the binary sensor indicates occupancy or presence.
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates occupancy/presence, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOccupied(this BinarySensorEntity sensor) => sensor.State.IsOn();

    /// <summary>
    /// Determines if the binary sensor indicates a clear state (no motion, no detection).
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates clear/no detection, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsClear(this BinarySensorEntity sensor) => sensor.State.IsOff();

    /// <summary>
    /// Determines if the binary sensor indicates a closed state (door, window, etc.).
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates closed state, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsClosed(this BinarySensorEntity sensor) => sensor.State.IsClosed();

    /// <summary>
    /// Determines if the binary sensor indicates a connected state (device connectivity).
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates connected state, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsConnected(this BinarySensorEntity sensor) => sensor.State.IsConnected();

    /// <summary>
    /// Determines if the binary sensor indicates a disconnected state (device connectivity).
    /// </summary>
    /// <param name="sensor">The binary sensor entity to check.</param>
    /// <returns>True if the sensor indicates disconnected state, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDisconnected(this BinarySensorEntity sensor) =>
        sensor.State.IsDisconnected();
}

/// <summary>
/// Extension methods for ClimateEntity providing convenient HVAC mode and state checking.
/// </summary>
public static class ClimateEntityExtensions
{
    /// <summary>
    /// Determines if the climate entity is in dry/dehumidify mode.
    /// </summary>
    /// <param name="climate">The climate entity to check.</param>
    /// <returns>True if the climate is in dry mode, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDry(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.DRY, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the climate entity is in cooling mode.
    /// </summary>
    /// <param name="climate">The climate entity to check.</param>
    /// <returns>True if the climate is in cooling mode, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCool(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.COOL, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the climate entity is turned off.
    /// </summary>
    /// <param name="climate">The climate entity to check.</param>
    /// <returns>True if the climate is off, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOff(this ClimateEntity climate) =>
        string.Equals(climate.State, HaEntityStates.OFF, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the climate entity is actively running (cooling or drying).
    /// </summary>
    /// <param name="climate">The climate entity to check.</param>
    /// <returns>True if the climate is actively running in any mode, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOn(this ClimateEntity climate) => climate.IsDry() || climate.IsCool();
}

/// <summary>
/// Extension methods for WeatherEntity providing comprehensive weather condition checking.
/// </summary>
public static class WeatherEntityExtensions
{
    /// <summary>
    /// Determines if the weather entity indicates dry conditions.
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather conditions are dry, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDry(this WeatherEntity climate) =>
        string.Equals(climate.State, HaEntityStates.DRY, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines if the weather entity indicates sunny or partly cloudy conditions.
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather is sunny or partly cloudy, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSunny(this WeatherEntity climate) =>
        climate.State is HaEntityStates.SUNNY or HaEntityStates.PARTLY_CLOUDY;

    /// <summary>
    /// Determines if the weather entity indicates rainy conditions (light to heavy rain).
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather includes any form of rain, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRainy(this WeatherEntity climate) =>
        climate.State
            is HaEntityStates.RAINY
                or HaEntityStates.POURING
                or HaEntityStates.LIGHTNING_RAINY;

    /// <summary>
    /// Determines if the weather entity indicates cloudy conditions.
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather is cloudy or partly cloudy, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCloudy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.CLOUDY or HaEntityStates.PARTLY_CLOUDY;

    /// <summary>
    /// Determines if the weather entity indicates clear night conditions.
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather is clear at night, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsClearNight(this WeatherEntity climate) =>
        string.Equals(
            climate.State,
            HaEntityStates.CLEAR_NIGHT,
            StringComparison.OrdinalIgnoreCase
        );

    /// <summary>
    /// Determines if the weather entity indicates stormy conditions (lightning, hail).
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather includes storms, lightning, or hail, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsStormy(this WeatherEntity climate) =>
        climate.State
            is HaEntityStates.LIGHTNING
                or HaEntityStates.LIGHTNING_RAINY
                or HaEntityStates.HAIL;

    /// <summary>
    /// Determines if the weather entity indicates snowy conditions.
    /// </summary>
    /// <param name="climate">The weather entity to check.</param>
    /// <returns>True if weather includes snow, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSnowy(this WeatherEntity climate) =>
        climate.State is HaEntityStates.SNOWY or HaEntityStates.SNOWY_RAINY;
}

/// <summary>
/// Extension methods for NumberEntity providing convenient value manipulation.
/// </summary>
public static class NumberEntityExtensions
{
    /// <summary>
    /// Sets the numeric value of a number entity using the Home Assistant service call.
    /// </summary>
    /// <param name="entity">The number entity to update.</param>
    /// <param name="value">The numeric value to set.</param>
    /// <remarks>
    /// This method calls the Home Assistant "set_value" service to update the number entity.
    /// The value should be within the entity's configured minimum and maximum range.
    /// </remarks>
    public static void SetNumericValue(this NumberEntity entity, double value)
    {
        entity.CallService("set_value", new { value });
    }
}

/// <summary>
/// Extension methods for LockEntity providing convenient lock state checking.
/// </summary>
public static class LockEntityExtensions
{
    /// <summary>
    /// Determines if the lock entity is in a locked state.
    /// </summary>
    /// <param name="entity">The lock entity to check.</param>
    /// <returns>True if the lock is locked, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLocked(this LockEntity entity) => entity.State.IsLocked();

    /// <summary>
    /// Determines if the lock entity is in an unlocked state.
    /// </summary>
    /// <param name="entity">The lock entity to check.</param>
    /// <returns>True if the lock is unlocked, false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUnlocked(this LockEntity entity) => entity.State.IsUnlocked();
}

/// <summary>
/// Extension methods for SwitchEntity providing advanced interaction patterns.
/// </summary>
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
