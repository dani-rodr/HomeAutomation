using System.Reactive.Subjects;
using System.Text.Json;

namespace HomeAutomation.Tests.Infrastructure;

/// <summary>
/// Captures service calls made during automation testing for verification
/// </summary>
public record ServiceCall(string Domain, string Service, ServiceTarget? Target, object? Data);

/// <summary>
/// Mock implementation of IHaContext for testing automation behavior
/// Allows capturing service calls and simulating state changes
/// </summary>
public class MockHaContext : IHaContext
{
    /// <summary>
    /// List of all service calls made during testing
    /// </summary>
    public List<ServiceCall> ServiceCalls { get; } = [];

    /// <summary>
    /// Subject for simulating state changes in tests
    /// </summary>
    public Subject<StateChange> StateChangeSubject { get; } = new();

    /// <summary>
    /// Subject for simulating events in tests
    /// </summary>
    public Subject<Event> EventSubject { get; } = new();

    /// <summary>
    /// Dictionary to track entity states for GetState() calls
    /// </summary>
    private readonly Dictionary<string, string> _entityStates = new();

    /// <summary>
    /// Dictionary to track entity attributes for testing
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, object>> _entityAttributes = new();

    // Core testing methods
    public void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        ServiceCalls.Add(new ServiceCall(domain, service, target, data));
    }

    public IObservable<StateChange> StateChanges() => StateChangeSubject.AsObservable();

    public IObservable<Event> Events => EventSubject.AsObservable();

    // Additional required interface members (basic implementations for testing)
    public IObservable<StateChange> StateAllChanges() => StateChangeSubject.AsObservable();

    public EntityState? GetState(string entityId)
    {
        var state = _entityStates.GetValueOrDefault(entityId, "unknown");
        return new EntityState { State = state };
    }

    public IReadOnlyList<Entity> GetAllEntities() => [];

    public Task<JsonElement?> CallServiceWithResponseAsync(
        string domain,
        string service,
        ServiceTarget? target = null,
        object? data = null
    )
    {
        // For testing, just record the call and return empty result
        CallService(domain, service, target, data);
        return Task.FromResult<JsonElement?>(null);
    }

    public NetDaemon.HassModel.Entities.Area? GetAreaFromEntityId(string entityId) => null;

    public EntityRegistration? GetEntityRegistration(string entityId) => null;

    public void SendEvent(string eventType, object? data = null)
    {
        // For testing, trigger the event through our subject
        var jsonElement = data != null ? JsonSerializer.SerializeToElement(data) : (JsonElement?)null;
        EventSubject.OnNext(new Event { EventType = eventType, DataElement = jsonElement });
    }

    /// <summary>
    /// Helper method to simulate an entity state change
    /// </summary>
    public void SimulateStateChange(string entityId, string oldState, string newState)
    {
        // Update tracked state so GetState() calls return the new state
        _entityStates[entityId] = newState;

        // Create and trigger state change event
        var stateChange = new StateChange(
            new Entity(this, entityId),
            new EntityState { State = oldState },
            new EntityState { State = newState }
        );
        StateChangeSubject.OnNext(stateChange);
    }

    /// <summary>
    /// Helper method to clear all captured service calls
    /// </summary>
    public void ClearServiceCalls() => ServiceCalls.Clear();

    /// <summary>
    /// Helper method to set an entity's initial state (useful for test setup)
    /// </summary>
    public void SetEntityState(string entityId, string state)
    {
        _entityStates[entityId] = state;
    }

    /// <summary>
    /// Helper method to set an entity's attributes (useful for test setup)
    /// </summary>
    public void SetEntityAttributes(string entityId, object attributes)
    {
        var attributeDict = new Dictionary<string, object>();
        foreach (var prop in attributes.GetType().GetProperties())
        {
            attributeDict[prop.Name] = prop.GetValue(attributes) ?? new object();
        }
        _entityAttributes[entityId] = attributeDict;
    }

    /// <summary>
    /// Helper method to get service calls for a specific domain
    /// </summary>
    public IEnumerable<ServiceCall> GetServiceCalls(string domain) => ServiceCalls.Where(call => call.Domain == domain);

    /// <summary>
    /// Helper method to get the most recent service call
    /// </summary>
    public ServiceCall? GetLastServiceCall() => ServiceCalls.LastOrDefault();

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        StateChangeSubject?.Dispose();
        EventSubject?.Dispose();
    }
}
