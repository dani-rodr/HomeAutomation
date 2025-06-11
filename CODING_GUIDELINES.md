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
├── /Helpers                # Utility functions and constants
└── /Security              # Security-related automations
```

### Class Naming Conventions
- **Automation Classes**: `{Area}Automation` (e.g., `KitchenAutomation`)
- **Base Classes**: `{Purpose}Base` (e.g., `MotionAutomationBase`)
- **Helper Classes**: Descriptive names (e.g., `HaIdentity`, `TimeRange`)
- **Constants**: `Ha{Category}` (e.g., `HaEntityStates`)

## 2. Class Structure Standards

### Automation Class Template
```csharp
/// <summary>
/// Handles automation logic for [specific area/purpose].
/// Manages [brief description of responsibilities].
/// </summary>
[NetDaemonApp]
public class ExampleAutomation(
    IHaContext ha,
    ILogger<ExampleAutomation> logger,
    INetDaemonScheduler scheduler) : AutomationBase(logger)
{
    private readonly Entities _entities = new(ha);
    private readonly INetDaemonScheduler _scheduler = scheduler;

    // Fields: private readonly, underscore prefix
    private readonly BinarySensorEntity _motionSensor = _entities.BinarySensor.ExampleMotion;

    // Properties: protected for inheritance, PascalCase
    protected LightEntity Light => _entities.Light.ExampleLight;

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

## 5. Async Patterns in Reactive Streams

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

## 6. Resource Management Standards

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

## 7. Thread Safety Guidelines

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

## 8. Documentation Standards

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

## 9. Logging Standards

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

## 10. Testing Standards

### Unit Test Structure
```csharp
[TestFixture]
public class MotionAutomationTests
{
    private Mock<IHaContext> _mockHa;
    private Mock<ILogger<MotionAutomation>> _mockLogger;
    private TestScheduler _testScheduler;
    private MotionAutomation _automation;

    [SetUp]
    public void Setup()
    {
        _mockHa = new Mock<IHaContext>();
        _mockLogger = new Mock<ILogger<MotionAutomation>>();
        _testScheduler = new TestScheduler();
        _automation = new MotionAutomation(_mockHa.Object, _mockLogger.Object, _testScheduler);
    }

    [Test]
    public void Motion_Detected_Should_Turn_On_Light()
    {
        // Arrange
        var motionObservable = _testScheduler.CreateHotObservable(
            ReactiveTest.OnNext(100, CreateStateChange("off", "on")),
            ReactiveTest.OnNext(300, CreateStateChange("on", "off"))
        );

        // Act
        _automation.StartAutomation();
        _testScheduler.AdvanceBy(500);

        // Assert
        // Verify light was turned on
        // Verify proper disposal of resources
    }

    [TearDown]
    public void TearDown()
    {
        _automation?.Dispose();
    }
}
```

### Testing Requirements
- **Test all public methods** and critical paths
- **Use TestScheduler** for time-based testing
- **Mock all external dependencies** (IHaContext, ILogger)
- **Test error conditions** and edge cases
- **Verify resource disposal** in all tests
- **Test thread safety** for concurrent operations

## 11. Modern C# 13/.NET 9 Features

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

## 12. Performance Guidelines

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

## 12. Code Review Checklist

### Before Submitting Code
- [ ] All classes have XML documentation
- [ ] All subscriptions have error handling
- [ ] Resources are properly disposed
- [ ] **Collection patterns follow strategic guidelines** ([] vs yield return vs [..])
- [ ] **Switch expressions used** for complex conditional logic where appropriate
- [ ] **Global usings utilized** - no redundant using statements
- [ ] **Modern C# 13 syntax** leveraged appropriately
- [ ] Thread safety is considered and documented
- [ ] Tests cover critical functionality
- [ ] Logging follows structured patterns
- [ ] Performance considerations addressed
- [ ] No hardcoded values (use constants/configuration)
- [ ] Follows established naming conventions
- [ ] No async lambdas in Subscribe() without error handling

### Architecture Review
- [ ] Appropriate base class chosen
- [ ] Single responsibility principle followed
- [ ] Dependencies injected properly
- [ ] Master switch pattern implemented correctly
- [ ] Reactive patterns used appropriately
- [ ] **NetDaemon subscription patterns preserved** (yield return for subscriptions)
- [ ] **Strategic collection usage** balances performance and readability

## 13. Common Anti-Patterns to Avoid

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

## Conclusion

These guidelines ensure consistent, maintainable, and reliable automation code. When in doubt, prioritize:

1. **Safety**: Proper error handling and resource management
2. **Clarity**: Clear naming and comprehensive documentation
3. **Performance**: Efficient reactive patterns and resource usage
4. **Testability**: Dependency injection and mockable designs

For questions or clarifications on these guidelines, refer to the NetDaemon documentation at https://netdaemon.xyz/ or consult the project's CLAUDE.md file.