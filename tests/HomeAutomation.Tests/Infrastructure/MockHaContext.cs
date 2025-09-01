using System.Text.Json;
using Microsoft.Reactive.Testing;

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
    public MockHaContext()
    {
        SchedulerProvider.Current = _testScheduler;
    }

    public IScheduler Scheduler => _testScheduler;

    public void AdvanceTimeBySeconds(int seconds) => AdvanceTimeBy(TimeSpan.FromSeconds(seconds));

    public void AdvanceTimeByMilliseconds(long milliseconds) =>
        AdvanceTimeBy(TimeSpan.FromMilliseconds(milliseconds));

    public void AdvanceTimeByMinutes(int minutes) => AdvanceTimeBy(TimeSpan.FromMinutes(minutes));

    public void AdvanceTimeByHours(int hours) => AdvanceTimeBy(TimeSpan.FromHours(hours));

    public void AdvanceTimeBy(TimeSpan timeSpan) => _testScheduler.AdvanceBy(timeSpan.Ticks);

    public void AdvanceTimeTo(DateTimeOffset time) => _testScheduler.AdvanceTo(time.Ticks);

    public static ILogger<T> CreateLogger<T>(LogLevel level)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(level).AddConsole();
        });

        return factory.CreateLogger<T>();
    }

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
    private readonly TestScheduler _testScheduler = new();

    /// <summary>
    /// Dictionary to track entity states for GetState() calls
    /// </summary>
    private readonly Dictionary<string, string> _entityStates = [];

    /// <summary>
    /// Dictionary to track entity attributes for testing
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, object>> _entityAttributes = [];

    /// <summary>
    /// Dictionary to track custom service responses for CallServiceWithResponseAsync
    /// </summary>
    private readonly Dictionary<string, JsonElement?> _serviceResponses = [];

    // Core testing methods
    public void CallService(
        string domain,
        string service,
        ServiceTarget? target = null,
        object? data = null
    )
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
        var attributes = _entityAttributes.GetValueOrDefault(entityId, []);

        // Convert attributes dictionary to JsonElement for proper deserialization
        var attributesJson = JsonSerializer.SerializeToElement(attributes);

        // Create EntityState with reflection since Attributes property is read-only
        var entityState = new EntityState { State = state };
        var attributesProperty = typeof(EntityState).GetProperty("Attributes");
        if (attributesProperty?.SetMethod != null)
        {
            attributesProperty.SetValue(entityState, attributesJson);
        }

        return entityState;
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

        // Check if we have a custom response for this service call
        var key = $"{domain}.{service}";
        if (data != null)
        {
            // For webostv commands, include the command in the key for specific responses
            var commandProperty = data.GetType().GetProperty("Command");
            if (commandProperty != null && commandProperty.GetValue(data) is string command)
            {
                key = $"{domain}.{service}.{command}";
            }
        }

        if (_serviceResponses.TryGetValue(key, out var response))
        {
            return Task.FromResult(response);
        }

        return Task.FromResult<JsonElement?>(null);
    }

    public NetDaemon.HassModel.Entities.Area? GetAreaFromEntityId(string entityId) => null;

    public EntityRegistration? GetEntityRegistration(string entityId) => null;

    public void SendEvent(string eventType, object? data = null)
    {
        // For testing, trigger the event through our subject
        var jsonElement =
            data != null ? JsonSerializer.SerializeToElement(data) : (JsonElement?)null;
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

    public void SimulateStateChange<TAttributes>(
        string entityId,
        string oldState,
        string newState,
        TAttributes attributes
    )
        where TAttributes : class
    {
        _entityStates[entityId] = newState;

        var attributesJson = JsonSerializer.SerializeToElement(attributes);

        var oldEntityState = new EntityState
        {
            EntityId = entityId,
            State = oldState,
            AttributesJson = JsonDocument.Parse("{}").RootElement,
        };

        var newEntityState = new EntityState
        {
            EntityId = entityId,
            State = newState,
            AttributesJson = attributesJson,
        };

        var stateChange = new StateChange(
            new Entity(this, entityId),
            oldEntityState,
            newEntityState
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
    public IEnumerable<ServiceCall> GetServiceCalls(string domain) =>
        ServiceCalls.Where(call => call.Domain == domain);

    /// <summary>
    /// Helper method to get the most recent service call
    /// </summary>
    public ServiceCall? GetLastServiceCall() => ServiceCalls.LastOrDefault();

    /// <summary>
    /// Helper method to set up custom responses for service calls
    /// </summary>
    public void SetServiceResponse(string domain, string service, string? command, object response)
    {
        var key = command != null ? $"{domain}.{service}.{command}" : $"{domain}.{service}";
        _serviceResponses[key] = JsonSerializer.SerializeToElement(response);
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        StateChangeSubject?.Dispose();
        EventSubject?.Dispose();
        SchedulerProvider.Reset();
    }
}
