using System.Text.Json;

namespace HomeAutomation.apps.Helpers;

public readonly struct StateChangeFluent<T, TState>(StateChange<T, TState> source, bool useNewState)
    where T : Entity
    where TState : EntityState
{
    private readonly StateChange<T, TState> _source = source;
    private readonly bool _useNewState = useNewState;

    private string? GetState() => _useNewState ? _source.New?.State : _source.Old?.State;

    public bool State(string expectedState) =>
        string.Equals(GetState(), expectedState, StringComparison.OrdinalIgnoreCase);

    public bool On() => State(HaEntityStates.ON);

    public bool Off() => State(HaEntityStates.OFF);

    public bool Locked() => State(HaEntityStates.LOCKED);

    public bool Unlocked() => State(HaEntityStates.UNLOCKED);

    public bool Open() => On();

    public bool Closed() => Off();

    public bool Unavailable() => State(HaEntityStates.UNAVAILABLE);
}

public static class StateChangeExtensions
{
    public static StateChangeFluent<T, TState> Is<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => new(e, useNewState: true);

    public static StateChangeFluent<T, TState> Was<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => new(e, useNewState: false);

    public static string UserId<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.New?.Context?.UserId ?? string.Empty;

    public static string State<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.New?.State ?? string.Empty;

    public static bool IsOn<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().On();

    public static bool IsOff<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Off();

    public static bool IsLocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Locked();

    public static bool IsUnlocked<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Unlocked();

    public static bool IsOpen<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Open();

    public static bool IsClosed<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Closed();

    public static bool IsUnavailable<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Unavailable();

    public static string UserId(this StateChange e) => e.New?.Context?.UserId ?? string.Empty;

    public static string Username(this StateChange e) => HaIdentity.GetName(e.UserId());

    public static bool IsValidButtonPress(this StateChange e) =>
        DateTime.TryParse(e?.New?.State, out _);

    public static bool IsManuallyOperated(this StateChange e) =>
        HaIdentity.IsManuallyOperated(e.UserId());

    public static bool IsPhysicallyOperated(this StateChange e) =>
        HaIdentity.IsPhysicallyOperated(e.UserId());

    public static bool IsAutomated(this StateChange e) => HaIdentity.IsAutomated(e.UserId());

    public static (T? Old, T? New) GetAttributeChange<T>(
        this StateChange change,
        string attributeName
    )
    {
        T? oldVal = TryGetAttributeValue<T>(change.Old?.Attributes, attributeName);
        T? newVal = TryGetAttributeValue<T>(change.New?.Attributes, attributeName);

        return (oldVal, newVal);
    }

    private static T? TryGetAttributeValue<T>(
        IReadOnlyDictionary<string, object>? attributes,
        string key
    )
    {
        if (attributes == null || !attributes.TryGetValue(key, out var value))
            return default;

        if (value is JsonElement json)
        {
            try
            {
                return json.Deserialize<T>();
            }
            catch
            {
                return default;
            }
        }

        try
        {
            return (T?)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
}

public static class StateExtensions
{
    public static bool IsOpen(this string? state) => state.IsOn();

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
