# HomeAutomation Coding Guidelines

## Overview

This document establishes coding standards and best practices for the HomeAutomation NetDaemon v5 project. These guidelines ensure consistency, maintainability, and reliability across all automation implementations.

## 1. Project Structure & Organization

### File Organization
```
/apps
├── /Area                    # Room-specific automations
│   ├── /Bathroom           # Single responsibility per area
│   ├── /Bedroom            # Consistent folder structure
│   └── ...
├── /Common                 # Shared base classes and interfaces
│   ├── /Base               # Abstract base classes
│   ├── /Containers         # Entity container interfaces
│   ├── /Interface          # Service interfaces
│   └── /Services           # Composition-based services
├── /Helpers                # Utility functions and constants
└── /Security              # Security-related automations
```

### Class Naming Conventions
- **Automation Classes**: `{Area}Automation` (e.g., `KitchenAutomation`)
- **Base Classes**: `{Purpose}Base` (e.g., `MotionAutomationBase`)
- **Helper Classes**: Descriptive names (e.g., `HaIdentity`, `TimeRange`)
- **Constants**: `Ha{Category}` (e.g., `HaEntityStates`)

## 2. Entity Container Architecture

### Entity Container Pattern
The codebase uses entity containers to group related Home Assistant entities and simplify dependency injection for testing:

```csharp
/// <summary>
/// Container for motion automation entities.
/// Simplifies constructor dependencies and enables easy testing.
/// </summary>
public interface IMotionAutomationEntities
{
    SwitchEntity MasterSwitch { get; }
    BinarySensorEntity MotionSensor { get; }
    LightEntity Light { get; }
    NumberEntity SensorDelay { get; }
}

public class BathroomMotionEntities(Entities entities) : IMotionAutomationEntities
{
    public SwitchEntity MasterSwitch => entities.Switch.BathroomAutomation;
    public BinarySensorEntity MotionSensor => entities.BinarySensor.BathroomMotion;
    public LightEntity Light => entities.Light.BathroomLights;
    public NumberEntity SensorDelay => entities.Number.BathroomMotionSensorActiveDelayValue;
}
```

### Shared Entity Containers
For entities used across multiple automations, create shared container interfaces:

```csharp
/// <summary>
/// Shared entities used by multiple LivingRoom automations.
/// Prevents duplication of entity declarations across containers.
/// </summary>
public interface ILivingRoomSharedEntities
{
    SwitchEntity StandFan { get; }
    BinarySensorEntity MotionSensor { get; }
    SwitchEntity MotionSensorSwitch { get; }
}

// Multiple automations can depend on shared entities
public interface ILivingRoomFanEntities : ILivingRoomSharedEntities
{
    SwitchEntity FanMasterSwitch { get; }
    NumericSensorEntity Pm25Sensor { get; }
}
```

### Entity Container Benefits
- **Simplified Testing**: Mock one container instead of 6+ individual entities
- **Clear Dependencies**: Explicit declaration of entity dependencies
- **Type Safety**: Compile-time validation of entity access
- **Maintainability**: Centralized entity mapping per area
- **Shared Entities**: Avoid duplication across multiple automations

## 3. Composition-Based Services

### Service Composition Pattern
Replace inheritance with composition for better testability and flexibility:

```csharp
/// <summary>
/// Dimming light controller service using composition.
/// Extracted from DimmingMotionAutomationBase for reusability.
/// </summary>
public class DimmingLightController(
    int sensorActiveDelayValue,
    NumberEntity sensorDelay,
    int dimBrightnessPct = 80,
    int dimDelaySeconds = 5) : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    public void StartDimming(LightEntity light, ILogger logger)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(dimDelaySeconds), _cancellationTokenSource.Token);
                light.TurnOn(brightness: dimBrightnessPct);
            }
            catch (TaskCanceledException)
            {
                logger.LogDebug("Dimming cancelled for {Light}", light.EntityId);
            }
        });
    }

    public void StopDimming() => _cancellationTokenSource?.Cancel();

    public void Dispose() => _cancellationTokenSource?.Dispose();
}
```

### Using Composition in Automations
```csharp
public class DimmingMotionAutomation(
    IMotionAutomationEntities entities,
    ILogger<DimmingMotionAutomation> logger) : MotionAutomationBase(entities, logger)
{
    private readonly DimmingLightController _dimmingController = new(
        sensorActiveDelayValue: 5,
        entities.SensorDelay,
        dimBrightnessPct: 80,
        dimDelaySeconds: 5);

    protected override void OnMotionDetected(StateChange evt)
    {
        base.OnMotionDetected(evt);
        _dimmingController.StartDimming(entities.Light, Logger);
    }

    public override void Dispose()
    {
        _dimmingController?.Dispose();
        base.Dispose();
    }
}
```

## 4. Class Structure Standards

### Modern Automation Class Template
```csharp
/// <summary>
/// Handles automation logic for [specific area/purpose].
/// Uses entity container pattern for improved testability.
/// </summary>
[NetDaemonApp]
public class ExampleAutomation(
    IExampleEntities entities,
    ILogger<ExampleAutomation> logger,
    INetDaemonScheduler scheduler) : AutomationBase(logger)
{
    private readonly IExampleEntities _entities = entities;
    private readonly INetDaemonScheduler _scheduler = scheduler;

    // Service composition for complex behaviors
    private readonly DimmingLightController _dimmingController = new(
        sensorActiveDelayValue: 5,
        entities.SensorDelay);

    // Properties: direct access to container entities
    protected BinarySensorEntity MotionSensor => _entities.MotionSensor;
    protected LightEntity Light => _entities.Light;

    /// <summary>
    /// Returns all switchable automations managed by the master switch.
    /// </summary>
    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        // Use yield return for active subscriptions
        yield return CreateMotionSubscription();
        yield return CreateScheduledTasks();
    }

    private IDisposable CreateMotionSubscription()
    {
        return _motionSensor
            .StateChanges()
            .Where(e => e.New.IsOn())
            .SubscribeSafe(OnMotionDetected, LogSubscriptionError);
    }

    private void OnMotionDetected(StateChange evt)
    {
        Logger.LogDebug("Motion detected in {Area}", nameof(ExampleAutomation));
        // Implementation here
    }

    private void LogSubscriptionError(Exception ex)
    {
        Logger.LogError(ex, "Error in {Automation} subscription", nameof(ExampleAutomation));
    }
}
```

## 3. Strategic Collection Patterns (Modern C# 13/.NET 9)

### Modern Collection Guidelines

**Use Collection Expressions `[]` when:**
- **Fixed small collections** (2-5 items): `GetFans() => [_fan1, _fan2, _fan3]`
- **Immediate materialization** required: `string[] modes = [AUTO, LOW, MEDIUM, HIGH]`
- **Type is obvious** from context: Array/List literals
- **Performance**: All items will be accessed immediately

**Use `yield return` when:**
- **NetDaemon subscription management** (critical for memory safety)
- **Large sequences** that benefit from lazy evaluation
- **Complex logic** between items
- **Memory-sensitive** operations

**Use Spread Syntax `[..]` when:**
- **Combining multiple sources**: `[..automations1, ..automations2]`
- **Materializing for specific containers**: CompositeDisposable
- **Short, known collections**: `[..base.GetItems(), newItem]`

### Implementation Examples

```csharp
// ✅ CORRECT: Collection expression for fixed entities
protected override IEnumerable<SwitchEntity> GetFans() =>
    [_ceilingFan, _standFan, _exhaustFan];

// ✅ CORRECT: Spread for combining automation sources
protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
    [.. GetLightAutomations(), .. GetSensorDelayAutomations(), .. GetAdditionalAutomations()];

// ✅ CORRECT: Collection expression for short subscription lists
protected override IEnumerable<IDisposable> GetLightAutomations() =>
    [
        MotionSensor.StateChanges().IsOn().Subscribe(_ => Light.TurnOn()),
        MotionSensor.StateChanges().IsOff().Subscribe(_ => Light.TurnOff()),
    ];

// ✅ CORRECT: Yield return for complex subscription management
protected override IEnumerable<IDisposable> GetSensorAutomations()
{
    yield return MotionSensor.StateChanges().Subscribe(HandleMotion);

    if (HasAdditionalSensors)
    {
        yield return CreateComplexSensorSubscription();
    }

    foreach (var sensor in GetDynamicSensors())
    {
        yield return sensor.StateChanges().Subscribe(HandleSensor);
    }
}

// ✅ CORRECT: Modern array initialization
string[] modes = [HaEntityStates.AUTO, HaEntityStates.LOW, HaEntityStates.MEDIUM];

// ❌ AVOID: Collection expression for NetDaemon subscription management with complex logic
protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
    [subscription1, subscription2]; // Prefer yield return for subscriptions
```

### Strategic Guidelines
- **NetDaemon subscriptions**: Always prefer `yield return` for proper lifecycle management
- **Fixed entity collections**: Use collection expressions for simplicity and readability
- **Combining sources**: Use spread syntax `[..]` for clean composition
- **Performance consideration**: Collection expressions materialize immediately; yield return provides lazy evaluation

## 4. Error Handling Standards

### Reactive Subscription Error Handling
```csharp
// ✅ CORRECT: Always handle errors in subscriptions
private IDisposable CreateSafeSubscription()
{
    return sensor
        .StateChanges()
        .Where(e => e.New.IsOn())
        .SubscribeSafe(HandleStateChange, LogSubscriptionError);
}

private void LogSubscriptionError(Exception ex)
{
    Logger.LogError(ex, "Subscription error in {Class}.{Method}",
        nameof(ExampleAutomation), nameof(CreateSafeSubscription));
}

// ✅ ALTERNATIVE: Manual try-catch pattern
private IDisposable CreateManualSubscription()
{
    return sensor.StateChanges().Subscribe(evt =>
    {
        try
        {
            HandleStateChange(evt);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling state change in {Entity}",
                evt.Entity.EntityId);
        }
    });
}

// ❌ INCORRECT: No error handling (can terminate stream)
private IDisposable CreateUnsafeSubscription()
{
    return sensor.StateChanges().Subscribe(HandleStateChange); // Dangerous!
}
```

### Error Handling Requirements
- **All subscriptions** MUST include error handling
- **Use structured logging** with appropriate context
- **Never let exceptions terminate** reactive streams
- **Consider fallback behavior** for critical automations

## 5. Interface-Based Design

### Event Handler Interfaces
Use interfaces for services to improve testability:

```csharp
/// <summary>
/// Interface for Home Assistant event handling.
/// Enables easy mocking and testing of event-driven automations.
/// </summary>
public interface IEventHandler
{
    void Subscribe(string eventType, Action<Event> handler);
    void Subscribe(string eventType, Action callback);
    IObservable<Event> WhenEventTriggered(string eventType);
}

public class HaEventHandler(IHaContext haContext, ILogger logger) : IEventHandler
{
    public void Subscribe(string eventType, Action<Event> handler)
    {
        haContext
            .Events.Where(e => e.EventType == eventType)
            .Subscribe(e =>
            {
                logger.LogInformation("Event '{EventType}' received", eventType);
                handler(e);
            });
    }

    public IObservable<Event> WhenEventTriggered(string eventType) =>
        haContext.Events.Where(e => e.EventType == eventType);
}
```

### Service Interface Guidelines
- **Create interfaces** for all complex services
- **Use dependency injection** with container registration
- **Mock interfaces** in unit tests
- **Keep interfaces focused** on single responsibility

## 6. Async Patterns in Reactive Streams

### Safe Async Patterns
```csharp
// ✅ CORRECT: Separate async method, proper error handling
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
{
    yield return MotionSensor
        .StateChanges()
        .IsOff()
        .SubscribeSafe(async evt => await OnMotionStoppedSafeAsync(evt), LogAsyncError);
}

private async Task OnMotionStoppedSafeAsync(StateChange evt)
{
    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await OnMotionStoppedAsync(cts.Token);
    }
    catch (TaskCanceledException)
    {
        Logger.LogDebug("Motion stopped task was cancelled");
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error in async motion stopped handler");
    }
}

// ❌ INCORRECT: Direct async lambda without error handling
yield return MotionSensor.StateChanges().IsOff()
    .Subscribe(async _ => await OnMotionStoppedAsync()); // Dangerous!
```

### Async Guidelines
- **Avoid async lambdas** directly in Subscribe()
- **Always include timeout** with CancellationToken
- **Handle TaskCanceledException** explicitly
- **Use proper error handling** for all async operations

## 7. Resource Management Standards

### IDisposable Implementation
```csharp
public class ExampleAutomation : AutomationBase
{
    private readonly CompositeDisposable _disposables = new();
    private IDisposable? _nonSwitchableSubscription;

    public override void StartAutomation()
    {
        base.StartAutomation(); // Handles switchable automations

        // Non-switchable subscriptions (always running)
        _nonSwitchableSubscription = CreateAlwaysOnSubscription();
    }

    protected override IEnumerable<IDisposable> GetSwitchableAutomations()
    {
        // These are managed by AutomationBase
        yield return CreateSwitchableSubscription();
    }

    public override void Dispose()
    {
        // Dispose non-switchable subscriptions first
        _nonSwitchableSubscription?.Dispose();

        // Base class handles switchable subscriptions
        base.Dispose();

        // Suppress finalizer
        GC.SuppressFinalize(this);
    }
}
```

### Resource Management Requirements
- **Always implement IDisposable** properly
- **Use CompositeDisposable** for multiple subscriptions
- **Separate switchable vs non-switchable** subscriptions
- **Call GC.SuppressFinalize(this)** in Dispose()
- **Dispose in correct order** (children first, then base)

## 8. Thread Safety Guidelines

### Shared State Management
```csharp
public class ThreadSafeAutomation : AutomationBase
{
    // ✅ CORRECT: Thread-safe field access
    private volatile bool _isProcessing = false;
    private readonly object _lock = new();
    private int _counter = 0;

    private void HandleConcurrentAccess()
    {
        // Simple boolean flags
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            // Complex state changes
            lock (_lock)
            {
                _counter++;
                ProcessWithSharedState();
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    // ✅ CORRECT: Atomic operations for simple types
    private void IncrementCounter()
    {
        Interlocked.Increment(ref _counter);
    }
}
```

### Thread Safety Requirements
- **Use `volatile`** for simple boolean flags
- **Use locks** for complex state changes
- **Use `Interlocked`** for atomic operations
- **Minimize lock scope** for performance
- **Document thread safety** assumptions

## 9. Documentation Standards

### XML Documentation Requirements
```csharp
/// <summary>
/// Manages motion-based lighting automation with dimming capabilities.
/// Automatically turns lights on/off based on motion detection and applies
/// appropriate brightness levels based on time of day.
/// </summary>
/// <remarks>
/// This automation extends <see cref="MotionAutomationBase"/> to add:
/// - Time-based dimming (bright during day, dim at night)
/// - Delayed turn-off with cancellation on re-entry
/// - Configurable motion sensor delay settings
/// </remarks>
[NetDaemonApp]
public class DimmingMotionAutomation : MotionAutomationBase
{
    /// <summary>
    /// Gets the delay in minutes before turning off lights after motion stops.
    /// </summary>
    /// <value>
    /// The delay in minutes. Default is 5 minutes for general areas,
    /// 2 minutes for high-traffic areas like hallways.
    /// </value>
    protected virtual int TurnOffDelayMinutes => 5;

    /// <summary>
    /// Handles motion detection events and applies appropriate lighting.
    /// </summary>
    /// <param name="evt">The state change event from the motion sensor.</param>
    /// <remarks>
    /// This method:
    /// 1. Cancels any pending turn-off operations
    /// 2. Determines appropriate brightness level based on time
    /// 3. Applies lighting settings to all configured lights
    /// </remarks>
    protected virtual void OnMotionDetected(StateChange evt)
    {
        // Implementation
    }
}
```

### Documentation Requirements
- **All public classes** require XML documentation
- **Include purpose and responsibilities** in class summary
- **Document complex methods** with parameters and behavior
- **Use `<remarks>`** for implementation details
- **Include examples** for base classes and complex APIs
- **Reference related classes** with `<see cref=""/>` tags

## 10. Logging Standards

### Structured Logging Pattern
```csharp
public class LoggingExampleAutomation : AutomationBase
{
    private void HandleStateChange(StateChange evt)
    {
        // ✅ CORRECT: Structured logging with context
        Logger.LogDebug("Processing state change for {Entity} from {OldState} to {NewState}",
            evt.Entity.EntityId, evt.Old?.State, evt.New?.State);

        try
        {
            ProcessChange(evt);

            Logger.LogInformation("Successfully processed {Entity} state change",
                evt.Entity.EntityId);
        }
        catch (ArgumentException ex)
        {
            Logger.LogWarning(ex, "Invalid argument for {Entity}: {Message}",
                evt.Entity.EntityId, ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to process state change for {Entity}",
                evt.Entity.EntityId);
            // Consider fallback behavior
        }
    }

    private void ScheduledTask()
    {
        // ✅ CORRECT: Performance logging for scheduled tasks
        Logger.LogDebug("Starting scheduled task {TaskName}", nameof(ScheduledTask));
        var stopwatch = Stopwatch.StartNew();

        try
        {
            PerformScheduledWork();
            Logger.LogDebug("Completed {TaskName} in {ElapsedMs}ms",
                nameof(ScheduledTask), stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in scheduled task {TaskName} after {ElapsedMs}ms",
                nameof(ScheduledTask), stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Logging Guidelines
- **Use structured logging** with named parameters
- **Include entity IDs** and relevant context
- **Log performance metrics** for long-running operations
- **Use appropriate log levels**:
  - `Debug`: Detailed flow information
  - `Information`: General operational messages
  - `Warning`: Unexpected but recoverable situations
  - `Error`: Failures that require attention
- **Don't log sensitive information** (tokens, passwords)

## 11. Testing Standards

### Entity Container Testing Pattern
```csharp
[TestFixture]
public class MotionAutomationTests
{
    private Mock<IMotionAutomationEntities> _mockEntities;
    private Mock<ILogger<MotionAutomation>> _mockLogger;
    private Mock<BinarySensorEntity> _mockMotionSensor;
    private Mock<LightEntity> _mockLight;
    private MotionAutomation _automation;

    [SetUp]
    public void Setup()
    {
        // Mock the entity container instead of individual entities
        _mockEntities = new Mock<IMotionAutomationEntities>();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();
        
        // Setup entity mocks
        _mockMotionSensor = new Mock<BinarySensorEntity>();
        _mockLight = new Mock<LightEntity>();
        
        // Configure container to return mocked entities
        _mockEntities.Setup(x => x.MotionSensor).Returns(_mockMotionSensor.Object);
        _mockEntities.Setup(x => x.Light).Returns(_mockLight.Object);
        
        _automation = new MotionAutomation(_mockEntities.Object, _mockLogger.Object);
    }

    [Test]
    public void Constructor_Should_UseEntityContainer_Successfully()
    {
        // Act - This tests that the container pattern works
        var automation = new MotionAutomation(_mockEntities.Object, _mockLogger.Object);

        // Assert
        automation.Should().NotBeNull();
        
        // Verify container was accessed during construction
        _mockEntities.Verify(x => x.MotionSensor, Times.AtLeastOnce);
        _mockEntities.Verify(x => x.Light, Times.AtLeastOnce);
    }
    
    [Test]
    public void EntityContainer_Should_SimplifyTesting()
    {
        // Before: Would need to mock 6+ individual entity parameters
        // After: Only need to mock 1 container interface
        
        // Arrange & Act
        var automation = new MotionAutomation(_mockEntities.Object, _mockLogger.Object);
        
        // Assert - Easy to verify which entities are used
        automation.Should().NotBeNull();
        _mockEntities.VerifyGet(x => x.MotionSensor, Times.AtLeastOnce);
    }

    [TearDown]
    public void TearDown()
    {
        _automation?.Dispose();
    }
}
```

### Entity Container Testing Benefits
- **Simplified Mocking**: Mock one container interface instead of 6+ entities
- **Clear Dependencies**: Entity requirements are explicit in interface
- **Type Safety**: Compile-time validation of entity access
- **Easy Verification**: Simple to verify which entities are used
- **Test Focus**: Test business logic, not entity wiring

### Testing Requirements
- **Use entity containers** for all automation tests
- **Mock container interfaces** instead of individual entities
- **Test critical automation paths** and error conditions
- **Verify entity interactions** through container mocks
- **Test service composition** separately from automation logic
- **Verify resource disposal** in all tests

## 12. Modern C# 13/.NET 9 Features

### Switch Expressions for Complex Logic
Replace complex if-else chains with switch expressions for better readability:

```csharp
// BEFORE: Complex nested conditionals
if (!occupied && doorOpen)
{
    return isColdWeather ? setting.NormalTemp : setting.UnoccupiedTemp;
}
if (occupied && !doorOpen)
{
    return setting.ClosedDoorTemp;
}
if (occupied && doorOpen && powerSaving)
{
    return setting.PowerSavingTemp;
}
return setting.NormalTemp;

// AFTER: Clean switch expression (implemented in ClimateAutomation)
return (occupied, doorOpen, powerSaving, isColdWeather) switch
{
    (false, true, _, true) => setting.NormalTemp,
    (false, true, _, false) => setting.UnoccupiedTemp,
    (true, false, _, _) => setting.ClosedDoorTemp,
    (true, true, true, _) => setting.PowerSavingTemp,
    _ => setting.NormalTemp
};
```

### Global Using Management
Centralize common usings in `GlobalUsings.cs` to reduce redundancy:

```csharp
// GlobalUsings.cs - Centralized common imports
global using System;
global using System.Collections.Generic;
global using System.Reactive.Concurrency;
global using System.Reactive.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using NetDaemon.Extensions.Scheduler;
global using HomeAssistantGenerated;
global using Microsoft.Extensions.Logging;
```

**Guidelines:**
- **Add to global usings** when 3+ files use the same namespace
- **Remove redundant usings** from individual files
- **Keep file-specific usings** for rarely used namespaces

### Target-Typed Expressions
Leverage modern C# type inference:

```csharp
// ✅ CORRECT: Target-typed new (already well-used in codebase)
_cachedSettings = new() { [key] = value };

// ✅ CORRECT: Target-typed collection expressions
string[] modes = [HaEntityStates.AUTO, HaEntityStates.LOW, HaEntityStates.MEDIUM];

// ✅ CORRECT: Inferred variable types where obvious
var entities = new Entities(ha);
var subscription = sensor.StateChanges().Subscribe(handler);
```

### Primary Constructors
Continue leveraging primary constructors (already well-implemented):

```csharp
// ✅ EXCELLENT: Already used throughout codebase
public class ExampleAutomation(
    IHaContext ha,
    ILogger<ExampleAutomation> logger,
    IScheduler scheduler) : AutomationBase(logger)
{
    private readonly Entities _entities = new(ha);
    private readonly IScheduler _scheduler = scheduler;
}
```

### Modern LINQ and Collection Methods
Use modern .NET performance features where appropriate:

```csharp
// ✅ String comparisons with proper culture (already implemented)
states.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase))

// ✅ Modern collection operations
collection.TryGetNonEnumeratedCount(out var count)
sequence.Chunk(batchSize)
items.DistinctBy(x => x.Key)
```

### File-Scoped Namespaces
Already well-implemented throughout the codebase:

```csharp
// ✅ CORRECT: File-scoped namespace (current standard)
namespace HomeAutomation.apps.Area.Bedroom.Automations;

public class BedroomAutomation : AutomationBase
{
    // Implementation
}
```

## 13. Performance Guidelines

### Reactive Stream Optimization
```csharp
// ✅ CORRECT: Efficient filtering and resource usage
private IDisposable CreateOptimizedSubscription()
{
    return _motionSensor
        .StateChanges()
        .Where(e => e.New.IsOn()) // Filter early
        .Throttle(TimeSpan.FromSeconds(1), _scheduler) // Debounce rapid changes
        .DistinctUntilChanged(e => e.New?.State) // Avoid duplicate processing
        .SubscribeSafe(HandleMotion, LogError);
}

// ✅ CORRECT: Cache expensive operations
private readonly Lazy<Dictionary<TimeBlock, AcScheduleSetting>> _scheduleCache =
    new(() => BuildScheduleSettings());

private Dictionary<TimeBlock, AcScheduleSetting> GetScheduleSettings() =>
    _scheduleCache.Value;

// ❌ INCORRECT: Recreating objects repeatedly
private Dictionary<TimeBlock, AcScheduleSetting> GetScheduleSettings() =>
    new() { /* expensive construction */ }; // Wasteful!
```

### Performance Requirements
- **Filter early** in reactive streams
- **Use throttling/debouncing** for rapid state changes
- **Cache expensive computations** appropriately
- **Avoid unnecessary object creation** in hot paths
- **Profile critical paths** for performance bottlenecks

## 14. Code Review Checklist

### Before Submitting Code
- [ ] All classes have XML documentation
- [ ] All subscriptions have error handling
- [ ] Resources are properly disposed
- [ ] **Entity containers used** for dependency injection
- [ ] **Composition preferred** over inheritance where appropriate
- [ ] **Service interfaces defined** for complex behaviors
- [ ] **Collection patterns follow strategic guidelines** ([] vs yield return vs [..])
- [ ] **Switch expressions used** for complex conditional logic where appropriate
- [ ] **Global usings utilized** - no redundant using statements
- [ ] **Modern C# 13 syntax** leveraged appropriately
- [ ] Thread safety is considered and documented
- [ ] Tests use entity container mocking
- [ ] Logging follows structured patterns
- [ ] Performance considerations addressed
- [ ] No hardcoded entity access (use containers)
- [ ] Follows established naming conventions
- [ ] No async lambdas in Subscribe() without error handling

### Architecture Review
- [ ] **Entity container pattern used** for all automation dependencies
- [ ] **Shared entity containers** used to avoid duplication
- [ ] **Composition over inheritance** applied where beneficial
- [ ] **Service interfaces** defined for testability
- [ ] Appropriate base class chosen
- [ ] Single responsibility principle followed
- [ ] Dependencies injected properly through containers
- [ ] Master switch pattern implemented correctly
- [ ] Reactive patterns used appropriately
- [ ] **NetDaemon subscription patterns preserved** (yield return for subscriptions)
- [ ] **Strategic collection usage** balances performance and readability
- [ ] **Tests use container mocking** instead of individual entity mocks

## 15. Common Anti-Patterns to Avoid

### ❌ Memory Leaks
```csharp
// WRONG: Subscription never disposed
public void BadMethod()
{
    _sensor.StateChanges().Subscribe(HandleChange); // Leak!
}
```

### ❌ Silent Failures
```csharp
// WRONG: Exception terminates stream permanently
_sensor.StateChanges().Subscribe(evt =>
{
    throw new Exception("Oops!"); // Stream dead forever!
});
```

### ❌ Blocking Operations
```csharp
// WRONG: Blocking the reactive stream
_sensor.StateChanges().Subscribe(evt =>
{
    Thread.Sleep(5000); // Blocks entire pipeline!
});
```

### ❌ Resource Contention
```csharp
// WRONG: Multiple subscriptions to same entity without coordination
_light.StateChanges().Subscribe(Handler1);
_light.StateChanges().Subscribe(Handler2); // Potential conflicts!
```

### ❌ Constructor Parameter Explosion
```csharp
// WRONG: Too many individual entity parameters
public ClimateAutomation(
    SwitchEntity masterSwitch,
    ClimateEntity ac,
    BinarySensorEntity houseSensor,
    SensorEntity tempSensor,
    SwitchEntity fanToggle,
    // ... 8 more parameters
) // Unmaintainable!
```

### ❌ Hardcoded Entity Access
```csharp
// WRONG: Direct entity access breaks container pattern
public MotionAutomation(IMotionEntities entities)
{
    var powerPlug = new Entities(ha).BinarySensor.SmartPlug3; // Breaks pattern!
}
```

### ❌ Duplicated Entity Declarations
```csharp
// WRONG: Same entities declared in multiple containers
public interface IFanEntities
{
    SwitchEntity StandFan { get; } // Duplicated!
}

public interface IMotionEntities  
{
    SwitchEntity StandFan { get; } // Duplicated!
}

// CORRECT: Use shared entity containers
public interface ISharedEntities
{
    SwitchEntity StandFan { get; }
}

public interface IFanEntities : ISharedEntities { }
public interface IMotionEntities : ISharedEntities { }
```

## Conclusion

These guidelines ensure consistent, maintainable, and reliable automation code. When in doubt, prioritize:

1. **Safety**: Proper error handling and resource management
2. **Clarity**: Clear naming and comprehensive documentation
3. **Performance**: Efficient reactive patterns and resource usage
4. **Testability**: Dependency injection and mockable designs

For questions or clarifications on these guidelines, refer to the NetDaemon documentation at https://netdaemon.xyz/ or consult the project's CLAUDE.md file.