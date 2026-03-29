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
    private static readonly JsonElement EmptyAttributes = JsonSerializer.SerializeToElement(
        new Dictionary<string, object?>()
    );

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
    private readonly Dictionary<string, JsonElement> _entityAttributes = [];

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
        var attributesJson = _entityAttributes.GetValueOrDefault(entityId, EmptyAttributes);

        var entityState = new EntityState { State = state };

        // Keep compatibility with model versions where computed Attributes needs explicit set.
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
        CallService(domain, service, target, data);

        var key = BuildServiceResponseKey(domain, service, data);

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
        EmitEvent(eventType, data);
    }

    public void EmitEvent(string eventType, object? data = null)
    {
        var jsonElement =
            data != null ? JsonSerializer.SerializeToElement(data) : (JsonElement?)null;
        EmitEvent(new Event { EventType = eventType, DataElement = jsonElement });
    }

    public void EmitEvent(Event @event)
    {
        EventSubject.OnNext(@event);
    }

    public void EmitStateChange(StateChange stateChange)
    {
        StateChangeSubject.OnNext(stateChange);
    }

    public void EmitMotionDetected(BinarySensorEntity motionSensor)
    {
        EmitStateChange(StateChangeHelpers.MotionDetected(motionSensor));
    }

    public void EmitMotionCleared(BinarySensorEntity motionSensor)
    {
        EmitStateChange(StateChangeHelpers.MotionCleared(motionSensor));
    }

    /// <summary>
    /// Helper method to simulate an entity state change
    /// </summary>
    public void SimulateStateChange(
        string entityId,
        string oldState,
        string newState,
        string? userId = null
    )
    {
        // Update tracked state so GetState() calls return the new state
        _entityStates[entityId] = newState;

        // Create and trigger state change event
        var stateChange = new StateChange(
            new Entity(this, entityId),
            new EntityState { State = oldState },
            new EntityState
            {
                State = newState,
                Context = new Context { UserId = userId },
            }
        );
        EmitStateChange(stateChange);
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

        EmitStateChange(stateChange);
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
        _entityAttributes[entityId] = JsonSerializer.SerializeToElement(attributes);
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
        var key = BuildServiceResponseKey(domain, service, null, command);
        _serviceResponses[key] = JsonSerializer.SerializeToElement(response);
    }

    private static string BuildServiceResponseKey(
        string domain,
        string service,
        object? data,
        string? commandOverride = null
    )
    {
        var command = commandOverride ?? TryGetCommand(data);
        return command is null ? $"{domain}.{service}" : $"{domain}.{service}.{command}";
    }

    private static string? TryGetCommand(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var commandProperty = data.GetType()
            .GetProperties()
            .FirstOrDefault(p =>
                string.Equals(p.Name, "Command", StringComparison.OrdinalIgnoreCase)
            );

        return commandProperty?.GetValue(data) as string;
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
