namespace HomeAutomation.apps.Helpers;

/// <summary>
/// Provides extension methods for state change validation, user identification, and state comparison.
/// These methods simplify state change processing in NetDaemon automations.
/// </summary>
public static class StateExtensions
{
    /// <summary>
    /// Extracts the user ID from the state change context.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>The user ID if available, otherwise an empty string.</returns>
    public static string UserId<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New?.Context?.UserId ?? string.Empty;
    }

    /// <summary>
    /// Extracts the state value from the state change.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>The state value if available, otherwise an empty string.</returns>
    public static string State<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New?.State ?? string.Empty;
    }

    /// <summary>
    /// Determines if the entity state change represents an "on" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "on", otherwise false.</returns>
    public static bool IsOn<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsOn();
    }

    /// <summary>
    /// Determines if the entity state change represents an "off" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "off", otherwise false.</returns>
    public static bool IsOff<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsOff();
    }

    /// <summary>
    /// Determines if the entity state change represents a "locked" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "locked", otherwise false.</returns>
    public static bool IsLocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsLocked();
    }

    /// <summary>
    /// Determines if the entity state change represents an "unlocked" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "unlocked", otherwise false.</returns>
    public static bool IsUnlocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsUnlocked();
    }

    public static bool IsOpen<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsOpen();
    }

    /// <summary>
    /// Determines if the entity state change represents an "unlocked" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "unlocked", otherwise false.</returns>
    public static bool IsClosed<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsClosed();
    }

    /// <summary>
    /// Determines if the entity state change represents an "unavailable" state.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity state type.</typeparam>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "unavailable", otherwise false.</returns>
    public static bool IsUnavailable<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState
    {
        return e.New != null && e.New.State.IsUnavailable();
    }

    /// <summary>
    /// Extracts the user ID from the state change context (non-generic version).
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>The user ID if available, otherwise an empty string.</returns>
    public static string UserId(this StateChange e)
    {
        return e.New?.Context?.UserId ?? string.Empty;
    }

    public static string Username(this StateChange e) => HaIdentity.GetName(e.UserId());

    /// <summary>
    /// Validates if the state change represents a valid button press event.
    /// A valid button press has a state value that can be parsed as a DateTime.
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the state represents a valid button press timestamp, otherwise false.</returns>
    public static bool IsValidButtonPress(this StateChange e) =>
        DateTime.TryParse(e?.New?.State, out _);

    /// <summary>
    /// Determines if the state change was triggered by manual user interaction.
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the change was manually operated by a user, otherwise false.</returns>
    public static bool IsManuallyOperated(this StateChange e) =>
        HaIdentity.IsManuallyOperated(e.UserId());

    /// <summary>
    /// Determines if the state change was triggered by physical device interaction.
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the change was physically operated, otherwise false.</returns>
    public static bool IsPhysicallyOperated(this StateChange e) =>
        HaIdentity.IsPhysicallyOperated(e.UserId());

    /// <summary>
    /// Determines if the state change was triggered by an automation.
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the change was automated, otherwise false.</returns>
    public static bool IsAutomated(this StateChange e) => HaIdentity.IsAutomated(e.UserId());

    /// <summary>
    /// Determines if the state change represents an "on" state (non-generic version).
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "on", otherwise false.</returns>
    public static bool IsOn(this StateChange e) => e?.New?.State?.IsOn() ?? false;

    /// <summary>
    /// Determines if the state change represents an "off" state (non-generic version).
    /// </summary>
    /// <param name="e">The state change event.</param>
    /// <returns>True if the new state is "off", otherwise returns true by default for null states.</returns>
    public static bool IsOff(this StateChange e) => e?.New?.State?.IsOff() ?? true;

    /// <summary>
    /// Determines if the state string represents an "open" state (alias for IsOn).
    /// </summary>
    /// <param name="state">The state string to check.</param>
    /// <returns>True if the state is "open" (on), otherwise false.</returns>
    public static bool IsOpen(this string? state) => state.IsOn();

    /// <summary>
    /// Determines if the state string represents a "closed" state (alias for IsOff).
    /// </summary>
    /// <param name="state">The state string to check.</param>
    /// <returns>True if the state is "closed" (off), otherwise false.</returns>
    public static bool IsClosed(this string? state) => state.IsOff();

    public static bool Is(this string? actual, string? toCheck) =>
        string.Equals(actual, toCheck, StringComparison.OrdinalIgnoreCase);

    public static bool IsLocked(this string? state) => state.Is(HaEntityStates.LOCKED);

    public static bool IsUnlocked(this string? state) => state.Is(HaEntityStates.UNLOCKED);

    public static bool IsHome(this string? state) => state.Is(HaEntityStates.HOME);

    public static bool IsAway(this string? state) => state.Is(HaEntityStates.AWAY);

    public static bool IsOn(this string? state) => state.Is(HaEntityStates.ON);

    public static bool IsOff(this string? state) => state.Is(HaEntityStates.OFF);

    public static bool IsOccupied(this string? state) => state.IsOn();

    public static bool IsCleared(this string? state) => state.IsOff();

    public static bool IsConnected(this string? state) => state.Is(HaEntityStates.CONNECTED);

    public static bool IsDisconnected(this string? state) => state.Is(HaEntityStates.DISCONNECTED);

    public static bool IsUnavailable(this string? state) => state.Is(HaEntityStates.UNAVAILABLE);

    public static bool IsUnknown(this string? state) => state.Is(HaEntityStates.UNKNOWN);

    public static bool IsAvailable(this string? state) =>
        !string.IsNullOrEmpty(state) && !state.IsUnavailable() && !state.IsUnknown();
}
