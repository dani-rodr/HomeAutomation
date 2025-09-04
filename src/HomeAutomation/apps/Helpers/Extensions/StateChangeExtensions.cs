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

    public static bool IsOn<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().On();

    public static bool IsOff<T, TState>(this StateChange<T, TState> e)
        where T : Entity
        where TState : EntityState => e.Is().Off();

    public static string UserId(this StateChange e) => e.New?.Context?.UserId ?? string.Empty;

    public static string Username(this StateChange e) => HaIdentity.GetName(e.UserId());

    public static bool IsPhysicallyOperated(this StateChange e) =>
        HaIdentity.IsPhysicallyOperated(e.UserId());

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
