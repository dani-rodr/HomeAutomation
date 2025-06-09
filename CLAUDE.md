# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a NetDaemon v5 home automation application that runs on Home Assistant. NetDaemon is a C# automation platform that enables developers to write home automations using modern .NET design patterns. It integrates with Home Assistant using websockets for maximum performance and provides strongly-typed entity access through code generation.

**Official Documentation**: https://netdaemon.xyz/  
**GitHub**: https://github.com/net-daemon/netdaemon

### Key Features
- **Modern .NET Development**: Write automations in C# using .NET 9
- **Reactive Programming**: Built on Reactive Extensions (Rx.NET) for event-driven programming
- **Code Generation**: Automatically generates strongly-typed entities and services from Home Assistant
- **High Performance**: WebSocket integration for real-time communication
- **Type Safety**: Full IntelliSense support with generated entity models

This project defines automations for various areas (bathroom, bedroom, kitchen, living room, pantry) and includes device controls, security features, and smart climate management.

## Key Commands

### Build & Deploy
```bash
# Standard build
dotnet build

# Publish to release
dotnet publish -c Release

# Deploy to Home Assistant (PowerShell)
.\publish.ps1
```

### Code Generation
```bash
# Generate Home Assistant entities and services from metadata
nd-codegen
```

### Dependency Management
```bash
# Update all dependencies (PowerShell)
.\update_all_dependencies.ps1

# Update code generator
dotnet tool update -g NetDaemon.HassModel.CodeGen
```

### WSL Development
```bash
# Use dotnet.exe when running from WSL
dotnet.exe build
dotnet.exe publish -c Release
```

## Architecture Overview

### Core Structure
- **`/apps`** - All automation applications
  - **`/Area`** - Room-specific automations (Bathroom, Bedroom, Kitchen, etc.)
  - **`/Common`** - Base classes and interfaces for all automations
  - **`/Security`** - Security-related automations (locks, location, notifications)
  - **`/Helpers`** - Constants and utility functions

### Key Base Classes
- **`AutomationBase`** - Abstract base for all automations with master switch support
- **`MotionAutomationBase`** - Base for motion-triggered automations
- **`DimmingMotionAutomationBase`** - Extended motion automation with dimming capabilities
- **`IAutomation`** - Interface all automations must implement

### Code Generation
The project uses NetDaemon's code generation to create strongly-typed entities and services:
- Generated code is in `HomeAssistantGenerated.cs`
- Metadata stored in `/NetDaemonCodegen/`
- Configuration in `appsettings.json` under `CodeGeneration` section
- Run `nd-codegen` to regenerate after adding new entities in Home Assistant

### Configuration
- **`appsettings.json`** - Main configuration (Home Assistant connection, logging)
- **`appsettings.Development.json`** - Development-specific settings (contains auth token)
- Home Assistant connection configured to `homeassistant.local:8123`

### Deployment
The `publish.ps1` script:
1. Stops the NetDaemon addon in Home Assistant
2. Publishes the project to the addon's config directory
3. Restarts the addon

### Development Requirements
- **.NET 9 SDK** with C# 13
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Home Assistant**: Access token with Administrator privileges
- **Git**: For version control (recommended)
- Follows strict EditorConfig rules (see `.editorconfig`)
- Global usings defined in `apps/GlobalUsings.cs`
- All automations use reactive extensions (System.Reactive)
- Master switches control automation groups

## NetDaemon Architecture Patterns & Best Practices

### NetDaemon Application Structure
NetDaemon applications follow specific patterns:
```csharp
[NetDaemonApp]
public class MyAutomation
{
    public MyAutomation(IHaContext ha, ILogger<MyAutomation> logger)
    {
        var entities = new Entities(ha);
        
        // Setup reactive subscriptions
        entities.BinarySensor.MotionSensor
            .StateChanges()
            .Where(e => e.New.IsOn())
            .Subscribe(_ => entities.Light.MyLight.TurnOn());
    }
}
```

### Project-Specific Inheritance Hierarchy
The project extends NetDaemon patterns with a custom hierarchy:
```
IAutomation (interface)
    └── AutomationBase (abstract base class)
        ├── Manages master switch functionality
        ├── Handles automation lifecycle (enable/disable)
        └── Implements IDisposable with CompositeDisposable
            └── MotionAutomationBase
                ├── Adds motion sensor logic
                ├── Controls sensor delay settings
                └── Manages light-motion relationships
                    └── DimmingMotionAutomationBase
                        ├── Adds dimming capabilities
                        ├── Implements delayed turn-off with cancellation
                        └── Uses CancellationTokenSource for async operations
```

### Reactive Programming Patterns (NetDaemon Best Practices)
- **State Changes**: Use `StateChanges()` for new state events only
- **Current State**: Use `StateChangesWithCurrent()` when you need the current state immediately  
- **Time-based Filters**: Use `WhenStateIsFor()` for proper time-delayed reactions
- **Safe Subscriptions**: Use `SubscribeSafe()` to prevent exceptions from breaking subscriptions
- **Subscriptions**: ALL subscriptions MUST be returned from `GetSwitchableAutomations()` or properly disposed

### Application Lifecycle (NetDaemon v5)
1. **Instantiating**: Constructor called, use for DI setup only
2. **Async Initialization**: Implement `IAsyncInitializable` for async setup
3. **Running**: App actively processing events
4. **Disposing**: Clean up resources when stopping
5. **Stopped**: App fully cleaned up

**Important**: Never block the constructor - use it only for setting up subscriptions

### Subscription Lifecycle Management
```csharp
// CORRECT: Subscriptions tracked and disposed
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
{
    yield return sensor.StateChanges().Subscribe(HandleChange);
    yield return scheduler.ScheduleCron("0 * * * *", RunHourly);
}

// INCORRECT: Creates memory leak
public override void StartAutomation()
{
    sensor.StateChanges().Subscribe(HandleChange); // Never disposed!
}
```

## Critical Implementation Notes

### NetDaemon-Specific Pitfalls to Avoid

1. **Memory Leaks from Untracked Subscriptions**
   - Problem: Creating subscriptions outside of `GetSwitchableAutomations()`
   - Solution: Always return IDisposable subscriptions from the method
   - NetDaemon Impact: Unhandled exceptions in subscriptions stop the entire stream
   - Found in: CookingAutomation.cs, ClimateAutomation.cs (cron job)

2. **Blocking Operations in Constructor**
   - Problem: NetDaemon expects constructor to return quickly
   - Solution: Move async/blocking operations to `IAsyncInitializable`
   - NetDaemon Impact: Can cause startup timeouts

3. **Thread Safety Issues**
   - Problem: Shared state accessed from multiple reactive streams
   - Solution: Use `volatile`, `Interlocked`, or lock synchronization
   - NetDaemon Impact: Rx.NET subscriptions may run on different threads
   - Example: `_isHouseEmpty` field in ClimateAutomation

4. **Unhandled Exceptions in Subscriptions**
   - Problem: Exceptions terminate the subscription permanently
   - Solution: Use `SubscribeSafe()` or wrap handlers in try-catch
   - NetDaemon Impact: Silent failures in automations

5. **Null Reference Exceptions**
   - Problem: Accessing nullable attributes without checks
   - Solution: Use null-conditional operators (`?.`) and proper null checks
   - Example: `_ac.Attributes?.Temperature` comparisons

6. **Time Zone Confusion**
   - Problem: Scheduler uses UTC, Cron expressions use local time
   - Solution: Be explicit about time zones in scheduling
   - NetDaemon Impact: Automations running at wrong times

### Proper Disposal Pattern
```csharp
public class MyAutomation : AutomationBase
{
    private IDisposable? _cronJob;
    
    public override void StartAutomation()
    {
        base.StartAutomation();
        _cronJob = scheduler.ScheduleCron("0 0 * * *", DoDaily);
    }
    
    public override void Dispose()
    {
        _cronJob?.Dispose(); // Dispose non-switchable subscriptions
        base.Dispose();      // Base handles switchable subscriptions
        GC.SuppressFinalize(this);
    }
}
```

### Pattern for Automations Without Master Switch
Some automations need to run continuously without master switch control:
```csharp
public class AlwaysOnAutomation : AutomationBase
{
    private readonly List<IDisposable> _subscriptions = new();
    
    public AlwaysOnAutomation(ILogger logger) : base(logger) // No master switch
    {
    }
    
    public override void StartAutomation()
    {
        // Don't call base.StartAutomation() - no master switch behavior needed
        _subscriptions.Add(CreateSubscription1());
        _subscriptions.Add(CreateSubscription2());
    }
    
    protected override IEnumerable<IDisposable> GetSwitchableAutomations() => [];
    
    public override void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

## Key Extension Methods & Helpers

### State Change Extensions (StateChangeObservableExtensions)
- `IsOn()`, `IsOff()` - Filter for on/off states
- `IsOpen()`, `IsClosed()` - Aliases for binary sensors
- `IsOnForSeconds(int)`, `IsOnForMinutes(int)` - Time-delayed state filters
- `WhenStateIsFor(predicate, timespan)` - Generic time-based filtering

### Entity Extensions
- `BinarySensorEntity.IsOccupied()` - Check occupancy state
- `ClimateEntity.IsOn()` - Returns true if cooling or drying
- `NumberEntity.SetNumericValue(double)` - Set number entity value
- `SensorEntity.LocalHour()` - Extract hour from datetime sensor

### User Action Validation (HaIdentity)
```csharp
// Check if action was manual (by user or physical switch)
if (HaIdentity.IsManuallyOperated(evt.UserId()))
{
    // User manually controlled the device
}

// Check if action was automated
if (HaIdentity.IsAutomated(evt.UserId()))
{
    // Action triggered by automation
}
```

### Time Utilities (TimeRange)
```csharp
// Check if current time is between hours
if (TimeRange.IsCurrentTimeInBetween(22, 6)) // 10 PM to 6 AM
{
    // Night time logic
}
```

## Known Issues & Solutions

### Issue: Memory Leak in Subscriptions
**Problem**: Subscriptions created in `StartAutomation()` are never disposed
```csharp
// WRONG
public override void StartAutomation()
{
    AutoTurnOffRiceCookerOnIdle(minutes: 10); // Leak!
}
```

**Solution**: Return subscriptions from `GetSwitchableAutomations()`
```csharp
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
{
    yield return AutoTurnOffRiceCookerOnIdle(minutes: 10);
}
```

### Issue: Race Conditions in Shared State
**Problem**: Multiple threads accessing shared fields
```csharp
private bool _isHouseEmpty = false; // Accessed from multiple streams
```

**Solution**: Use thread-safe patterns
```csharp
private volatile bool _isHouseEmpty = false;
// OR
private readonly object _lock = new();
private bool _isHouseEmpty;
```

### Issue: Inefficient Resource Usage
**Problem**: Recreating objects on every call
```csharp
private Dictionary<TimeBlock, AcScheduleSetting> GetSettings() => new() { ... };
```

**Solution**: Cache when appropriate
```csharp
private Dictionary<TimeBlock, AcScheduleSetting>? _cachedSettings;
private Dictionary<TimeBlock, AcScheduleSetting> GetSettings() => 
    _cachedSettings ??= BuildSettings();
```

## Development Guidelines

### Implementing GetSwitchableAutomations()
This method is crucial for proper resource management:
```csharp
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
{
    // Return ALL subscriptions that should be managed by master switch
    yield return sensor.StateChanges().Subscribe(Handle);
    yield return scheduler.ScheduleCron("0 * * * *", Hourly);
    
    // Chain multiple collections if needed
    foreach (var sub in GetAdditionalSubscriptions())
        yield return sub;
}
```

### State Change Patterns (NetDaemon v5)
```csharp
// React to state changes only
sensor.StateChanges().IsOn().Subscribe(TurnOnLight);

// Include current state in initial subscription
sensor.StateChangesWithCurrent().Where(s => s.IsOn()).Subscribe(Configure);

// Time-based reactions (NetDaemon pattern)
sensor.StateChanges()
    .WhenStateIsFor(s => s?.State == "off", TimeSpan.FromMinutes(10), scheduler)
    .Subscribe(_ => TurnOffLights());

// Safe subscription to handle exceptions
sensor.StateChanges()
    .Where(e => e.New.IsOn())
    .SubscribeSafe(HandleMotion, ex => Logger.LogError(ex, "Motion handler failed"));

// Ignore events during disposal
sensor.StateChanges()
    .Where(e => e.New.IsOn())
    .IgnoreOnComplete() // Prevents actions during app shutdown
    .Subscribe(HandleMotion);
```

### Error Handling Best Practices
```csharp
protected virtual void HandleStateChange(StateChange evt)
{
    try
    {
        // Your logic here
        ProcessChange(evt);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Failed to handle state change for {Entity}", 
            evt.Entity.EntityId);
        // Consider fallback behavior
    }
}
```

### Logging Guidelines
- Use structured logging with appropriate levels
- Log automation decisions at Debug level
- Log errors and warnings appropriately
- Include entity IDs and states in log context

```csharp
Logger.LogDebug("Applying AC settings for {TimeBlock} with temp {Temp}°C", 
    timeBlock, targetTemp);
    
Logger.LogWarning("Invalid hour {Hour} for schedule", hour);

Logger.LogError(ex, "Failed to control {Device}", device.EntityId);
```

## Testing Guidelines

NetDaemon provides excellent testing support through reactive testing utilities:

### Time-Based Testing
```csharp
using Microsoft.Reactive.Testing;

[Test]
public void Motion_Should_Turn_Off_After_Delay()
{
    var testScheduler = new TestScheduler();
    var motion = testScheduler.CreateHotObservable(
        OnNext(100, new StateChange(null, "on", null)),
        OnNext(200, new StateChange("on", "off", null))
    );
    
    // Test automation behavior with controlled time
    testScheduler.AdvanceBy(TimeSpan.FromMinutes(10).Ticks);
}
```

### Best Practices for Testing
- Test all edge cases and error conditions
- Use `TestScheduler` for time-based testing
- Mock Home Assistant entities and state changes
- Verify proper disposal of resources
- Test thread safety with concurrent operations

## Deployment & Installation

### Home Assistant Add-on (Recommended)
1. Add repository: `https://github.com/net-daemon/homeassistant-addon`
2. Install NetDaemon add-on from the store
3. Deploy apps to `/config/netdaemon5` directory
4. Configure connection in add-on settings

### Docker Deployment
```yaml
version: '3.7'
services:
  netdaemon:
    image: netdaemon/netdaemon5
    environment:
      - HomeAssistant__Host=homeassistant.local
      - HomeAssistant__Port=8123
      - HomeAssistant__Token=YOUR_TOKEN
    volumes:
      - ./apps:/app
```

### Required Home Assistant Configuration
- Long-lived access token with Administrator privileges
- WebSocket API enabled (default in Home Assistant)
- Network access between NetDaemon and Home Assistant