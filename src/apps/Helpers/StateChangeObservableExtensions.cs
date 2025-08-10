using System.Linq;
using System.Runtime.CompilerServices;

namespace HomeAutomation.apps.Helpers;

/// <summary>
/// Specifies the time unit for time-based state persistence checks.
/// </summary>
public enum TimeUnit
{
    /// <summary>Time measured in seconds.</summary>
    Seconds,

    /// <summary>Time measured in minutes.</summary>
    Minutes,

    /// <summary>Time measured in hours.</summary>
    Hours,
}

/// <summary>
/// Provides reactive extension methods for filtering and transforming Home Assistant state changes.
/// These methods enable fluent, declarative automation logic for NetDaemon applications.
/// </summary>
/// <remarks>
/// This class contains performance-optimized extensions for common automation patterns:
/// - Basic state filtering (IsOn, IsOff, IsOpen, IsClosed, etc.)
/// - Time-based state persistence checks (IsOnForMinutes, WhenStateIsForHours, etc.)
/// - User action filtering (IsManuallyOperated, IsPhysicallyOperated, IsAutomated)
/// - Advanced state detection (IsFlickering, IsValidButtonPress)
/// - Strongly-typed generic extensions for type-safe state changes
///
/// All simple state check methods use aggressive inlining for optimal performance.
/// String comparisons use ordinal case-insensitive comparison for consistency.
/// </remarks>
public static class StateChangeObservableExtensions
{
    /// <summary>
    /// Filters state changes to only emit when the new state matches any of the specified states.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="states">The states to match against (case-insensitive).</param>
    /// <returns>An observable that emits state changes when the new state matches any specified state.</returns>
    /// <remarks>
    /// This method filters out unavailable old states and null states for reliability.
    /// Uses optimized string comparison with StringComparison.OrdinalIgnoreCase.
    /// </remarks>
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

    /// <summary>
    /// Filters state changes to only emit when the entity turns on.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "on".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOn(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.ON);

    /// <summary>
    /// Filters state changes to only emit when the entity opens (alias for IsOn).
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "on" (open).</returns>
    /// <remarks>Commonly used for door sensors, window sensors, and other binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOpen(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsOn(ignorePreviousUnavailable);

    /// <summary>
    /// Filters state changes to only emit when the entity turns off.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "off".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOff(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.OFF);

    /// <summary>
    /// Filters state changes to only emit when the entity closes (alias for IsOff).
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "off" (closed).</returns>
    /// <remarks>Commonly used for door sensors, window sensors, and other binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsClosed(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsOff(ignorePreviousUnavailable);

    /// <summary>
    /// Filters state changes to only emit when the entity becomes locked.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "locked".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsLocked(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.LOCKED);

    public static IObservable<StateChange> IsActive(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.ACTIVE);

    public static IObservable<StateChange> IsIdle(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.IDLE);

    /// <summary>
    /// Filters state changes to only emit when the entity becomes unlocked.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "unlocked".</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnlocked(
        this IObservable<StateChange> source,
        bool ignorePreviousUnavailable = true
    ) => source.IsAnyOfStates(ignorePreviousUnavailable, HaEntityStates.UNLOCKED);

    /// <summary>
    /// Filters state changes to only emit when the entity becomes unavailable.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "unavailable".</returns>
    /// <remarks>Useful for monitoring device connectivity and handling offline scenarios.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Filters state changes to only emit when the entity state becomes unknown.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the entity state changes to "unknown".</returns>
    /// <remarks>Useful for detecting initialization states or sensor malfunctions.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    /// <summary>
    /// Filters state changes to only emit when the change was triggered by manual user operation.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the state change was manually triggered by a user.</returns>
    /// <remarks>
    /// Manual operations include actions through Home Assistant UI, mobile apps, or voice assistants.
    /// Uses HaIdentity.IsManuallyOperated() to determine user context.
    /// </remarks>
    public static IObservable<StateChange> IsManuallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsManuallyOperated(s.UserId()));

    /// <summary>
    /// Filters state changes to only emit when the change was triggered by physical interaction.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the state change was physically triggered.</returns>
    /// <remarks>
    /// Physical operations include wall switches, physical buttons, and hardware controls.
    /// Uses HaIdentity.IsPhysicallyOperated() to determine interaction context.
    /// </remarks>
    public static IObservable<StateChange> IsPhysicallyOperated(
        this IObservable<StateChange> source
    ) => source.Where(s => HaIdentity.IsPhysicallyOperated(s.UserId()));

    /// <summary>
    /// Filters state changes to only emit when the change was triggered by automation.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when the state change was triggered by automation systems.</returns>
    /// <remarks>
    /// Automated operations include NetDaemon automations, Home Assistant automations, and scripts.
    /// Uses HaIdentity.IsAutomated() to determine automation context.
    /// </remarks>
    public static IObservable<StateChange> IsAutomated(this IObservable<StateChange> source) =>
        source.Where(s => HaIdentity.IsAutomated(s.UserId()));

    /// <summary>
    /// Filters state changes to only emit valid button press events.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <returns>An observable that emits when a valid button press is detected.</returns>
    /// <remarks>
    /// Button presses are validated by checking if the state can be parsed as a DateTime.
    /// This is commonly used with Zigbee buttons and other event-based devices.
    /// </remarks>
    public static IObservable<StateChange> IsValidButtonPress(
        this IObservable<StateChange> source
    ) => source.Where(s => s.IsValidButtonPress());

    /// <summary>
    /// Core unified method for time-based state persistence filtering.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="desiredState">The state that must persist.</param>
    /// <param name="time">The time value the state must persist.</param>
    /// <param name="timeUnit">The unit of time (seconds, minutes, or hours).</param>
    /// <returns>An observable that emits when the state persists for the specified duration.</returns>
    /// <remarks>
    /// This unified method replaces the repetitive WhenStateIsForSeconds/Minutes/Hours pattern.
    /// Uses optimized string comparison with StringComparison.OrdinalIgnoreCase.
    /// </remarks>
    private static IObservable<StateChange> WhenStateIsFor(
        this IObservable<StateChange> source,
        string desiredState,
        int time,
        TimeUnit timeUnit
    )
    {
        var timeSpan = timeUnit switch
        {
            TimeUnit.Seconds => TimeSpan.FromSeconds(time),
            TimeUnit.Minutes => TimeSpan.FromMinutes(time),
            TimeUnit.Hours => TimeSpan.FromHours(time),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit)),
        };

        return source.WhenStateIsFor(
            s => string.Equals(s?.State, desiredState, StringComparison.OrdinalIgnoreCase),
            timeSpan,
            SchedulerProvider.Current
        );
    }

    /// <summary>
    /// Core unified method for entity state + time persistence filtering.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="entityState">The Home Assistant entity state constant.</param>
    /// <param name="time">The time value the state must persist.</param>
    /// <param name="timeUnit">The unit of time (seconds, minutes, or hours).</param>
    /// <returns>An observable that emits when the entity stays in the specified state for the duration.</returns>
    /// <remarks>
    /// This unified method replaces the 18 repetitive state-specific time methods (IsOnForMinutes, etc.).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IObservable<StateChange> ForEntityStateAndTime(
        this IObservable<StateChange> source,
        string entityState,
        int time,
        TimeUnit timeUnit
    ) => source.WhenStateIsFor(entityState, time, timeUnit);

    /// <summary>
    /// Core unified method for generic time-based state persistence filtering.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="source">The source observable of strongly-typed state changes.</param>
    /// <param name="predicate">A function to test the state for the desired condition.</param>
    /// <param name="time">The time value the condition must persist.</param>
    /// <param name="timeUnit">The unit of time (seconds, minutes, or hours).</param>
    /// <returns>An observable that emits when the condition persists for the specified duration.</returns>
    /// <remarks>
    /// This unified method replaces the repetitive generic WhenStateIsForSeconds/Minutes/Hours pattern.
    /// </remarks>
    private static IObservable<StateChange<T, TState>> WhenStateIsFor<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time,
        TimeUnit timeUnit
    )
        where T : Entity
        where TState : EntityState
    {
        var timeSpan = timeUnit switch
        {
            TimeUnit.Seconds => TimeSpan.FromSeconds(time),
            TimeUnit.Minutes => TimeSpan.FromMinutes(time),
            TimeUnit.Hours => TimeSpan.FromHours(time),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit)),
        };

        return source.WhenStateIsFor(predicate, timeSpan, SchedulerProvider.Current);
    }

    /// <summary>
    /// Emits state changes only when the entity remains in the specified state for the given number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="desiredState">The state that must persist.</param>
    /// <param name="time">The number of seconds the state must persist.</param>
    /// <returns>An observable that emits when the state persists for the specified duration.</returns>
    /// <remarks>
    /// Uses WhenStateIsFor internally with TimeSpan.FromSeconds for time-based filtering.
    /// Essential for debouncing rapid state changes in motion sensors and other noisy devices.
    /// </remarks>
    public static IObservable<StateChange> WhenStateIsForSeconds(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(desiredState, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains in the specified state for the given number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="desiredState">The state that must persist.</param>
    /// <param name="time">The number of minutes the state must persist.</param>
    /// <returns>An observable that emits when the state persists for the specified duration.</returns>
    /// <remarks>
    /// Uses WhenStateIsFor internally with TimeSpan.FromMinutes for time-based filtering.
    /// Commonly used for occupancy detection and delayed automation triggers.
    /// </remarks>
    public static IObservable<StateChange> WhenStateIsForMinutes(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(desiredState, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains in the specified state for the given number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="desiredState">The state that must persist.</param>
    /// <param name="time">The number of hours the state must persist.</param>
    /// <returns>An observable that emits when the state persists for the specified duration.</returns>
    /// <remarks>
    /// Uses WhenStateIsFor internally with TimeSpan.FromHours for time-based filtering.
    /// Useful for long-duration monitoring and energy management automations.
    /// </remarks>
    public static IObservable<StateChange> WhenStateIsForHours(
        this IObservable<StateChange> source,
        string desiredState,
        int time
    ) => source.WhenStateIsFor(desiredState, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "on" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain on.</param>
    /// <returns>An observable that emits when the entity stays on for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOnForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "on" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain on.</param>
    /// <returns>An observable that emits when the entity stays on for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOnForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "on" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain on.</param>
    /// <returns>An observable that emits when the entity stays on for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOnForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "off" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain off.</param>
    /// <returns>An observable that emits when the entity stays off for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOffForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "off" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain off.</param>
    /// <returns>An observable that emits when the entity stays off for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOffForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "off" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain off.</param>
    /// <returns>An observable that emits when the entity stays off for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOffForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "closed" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain closed.</param>
    /// <returns>An observable that emits when the entity stays closed for the specified duration.</returns>
    /// <remarks>Closed state is equivalent to "off" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsClosedForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "closed" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain closed.</param>
    /// <returns>An observable that emits when the entity stays closed for the specified duration.</returns>
    /// <remarks>Closed state is equivalent to "off" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsClosedForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "closed" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain closed.</param>
    /// <returns>An observable that emits when the entity stays closed for the specified duration.</returns>
    /// <remarks>Closed state is equivalent to "off" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsClosedForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.OFF, time, TimeUnit.Hours);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnavailableForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNAVAILABLE, time, TimeUnit.Seconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnavailableForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNAVAILABLE, time, TimeUnit.Minutes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnavailableForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNAVAILABLE, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "open" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain open.</param>
    /// <returns>An observable that emits when the entity stays open for the specified duration.</returns>
    /// <remarks>Open state is equivalent to "on" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOpenForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "open" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain open.</param>
    /// <returns>An observable that emits when the entity stays open for the specified duration.</returns>
    /// <remarks>Open state is equivalent to "on" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOpenForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "open" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain open.</param>
    /// <returns>An observable that emits when the entity stays open for the specified duration.</returns>
    /// <remarks>Open state is equivalent to "on" state for most binary sensors.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsOpenForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.ON, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "locked" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain locked.</param>
    /// <returns>An observable that emits when the entity stays locked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsLockedForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.LOCKED, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "locked" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain locked.</param>
    /// <returns>An observable that emits when the entity stays locked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsLockedForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.LOCKED, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "locked" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain locked.</param>
    /// <returns>An observable that emits when the entity stays locked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsLockedForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.LOCKED, time, TimeUnit.Hours);

    /// <summary>
    /// Emits state changes only when the entity remains "unlocked" for the specified number of seconds.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of seconds the entity must remain unlocked.</param>
    /// <returns>An observable that emits when the entity stays unlocked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnlockedForSeconds(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNLOCKED, time, TimeUnit.Seconds);

    /// <summary>
    /// Emits state changes only when the entity remains "unlocked" for the specified number of minutes.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of minutes the entity must remain unlocked.</param>
    /// <returns>An observable that emits when the entity stays unlocked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnlockedForMinutes(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNLOCKED, time, TimeUnit.Minutes);

    /// <summary>
    /// Emits state changes only when the entity remains "unlocked" for the specified number of hours.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="time">The number of hours the entity must remain unlocked.</param>
    /// <returns>An observable that emits when the entity stays unlocked for the specified duration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IObservable<StateChange> IsUnlockedForHours(
        this IObservable<StateChange> source,
        int time
    ) => source.ForEntityStateAndTime(HaEntityStates.UNLOCKED, time, TimeUnit.Hours);

    /// <summary>
    /// Detects flickering behavior in entity state changes over a specified time window.
    /// </summary>
    /// <param name="source">The source observable of state changes.</param>
    /// <param name="minimumFlips">The minimum number of state changes required to be considered flickering (default: 4).</param>
    /// <param name="timeWindowMs">The time window in seconds to observe for flickering behavior (default: 10).</param>
    /// <returns>An observable that emits a list of state changes when flickering is detected.</returns>
    /// <remarks>
    /// Flickering detection is useful for identifying malfunctioning sensors, unstable network connections,
    /// or other issues causing rapid state changes. The method uses DistinctUntilChanged to ignore
    /// duplicate consecutive states and Buffer to collect changes within the time window.
    /// </remarks>
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

    /// <summary>
    /// Strongly-typed extension for waiting until a state persists for a specified number of seconds.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="source">The source observable of strongly-typed state changes.</param>
    /// <param name="predicate">A function to test the state for the desired condition.</param>
    /// <param name="time">The number of seconds the condition must persist.</param>
    /// <returns>An observable that emits when the condition persists for the specified duration.</returns>
    /// <remarks>
    /// This generic version provides type safety and IntelliSense support for strongly-typed entities.
    /// Use this when working with generated entity types for better development experience.
    /// </remarks>
    public static IObservable<StateChange<T, TState>> WhenStateIsForSeconds<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, time, TimeUnit.Seconds);
    }

    /// <summary>
    /// Strongly-typed extension for waiting until a state persists for a specified number of minutes.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="source">The source observable of strongly-typed state changes.</param>
    /// <param name="predicate">A function to test the state for the desired condition.</param>
    /// <param name="time">The number of minutes the condition must persist.</param>
    /// <returns>An observable that emits when the condition persists for the specified duration.</returns>
    /// <remarks>
    /// This generic version provides type safety and IntelliSense support for strongly-typed entities.
    /// Use this when working with generated entity types for better development experience.
    /// </remarks>
    public static IObservable<StateChange<T, TState>> WhenStateIsForMinutes<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, time, TimeUnit.Minutes);
    }

    /// <summary>
    /// Strongly-typed extension for waiting until a state persists for a specified number of hours.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="source">The source observable of strongly-typed state changes.</param>
    /// <param name="predicate">A function to test the state for the desired condition.</param>
    /// <param name="time">The number of hours the condition must persist.</param>
    /// <returns>An observable that emits when the condition persists for the specified duration.</returns>
    /// <remarks>
    /// This generic version provides type safety and IntelliSense support for strongly-typed entities.
    /// Use this when working with generated entity types for better development experience.
    /// </remarks>
    public static IObservable<StateChange<T, TState>> WhenStateIsForHours<T, TState>(
        this IObservable<StateChange<T, TState>> source,
        Func<TState?, bool> predicate,
        int time
    )
        where T : Entity
        where TState : EntityState
    {
        return source.WhenStateIsFor(predicate, time, TimeUnit.Hours);
    }
}
