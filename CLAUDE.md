# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a NetDaemon v5 home automation application that runs on Home Assistant. NetDaemon allows writing Home Assistant automations in C# (.NET 9). The project defines automations for various areas (bathroom, bedroom, kitchen, living room, pantry) and includes device controls, security features, and smart climate management.

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

### Configuration
- **`appsettings.json`** - Main configuration (Home Assistant connection, logging)
- **`appsettings.Development.json`** - Development-specific settings (contains auth token)
- Home Assistant connection configured to `homeassistant.local:8123`

### Deployment
The `publish.ps1` script:
1. Stops the NetDaemon addon in Home Assistant
2. Publishes the project to the addon's config directory
3. Restarts the addon

### Development Notes
- Uses .NET 9 with C# 13
- Follows strict EditorConfig rules (see `.editorconfig`)
- Global usings defined in `apps/GlobalUsings.cs`
- All automations use reactive extensions (System.Reactive)
- Master switches control automation groups

## Architecture Patterns & Best Practices

### Inheritance Hierarchy
The project follows a well-structured inheritance pattern:
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

### Reactive Programming Patterns
- **State Changes**: Use `StateChanges()` for new state events only
- **Current State**: Use `StateChangesWithCurrent()` when you need the current state immediately
- **Time-based Filters**: Chain methods like `IsOnForMinutes(5)` for delayed reactions
- **Subscriptions**: ALL subscriptions MUST be returned from `GetSwitchableAutomations()` or properly disposed

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

### Common Pitfalls to Avoid

1. **Memory Leaks from Untracked Subscriptions**
   - Problem: Creating subscriptions outside of `GetSwitchableAutomations()`
   - Solution: Always return IDisposable subscriptions from the method
   - Found in: CookingAutomation.cs, ClimateAutomation.cs (cron job)

2. **Thread Safety Issues**
   - Problem: Shared state accessed from multiple reactive streams
   - Solution: Use `volatile`, `Interlocked`, or lock synchronization
   - Example: `_isHouseEmpty` field in ClimateAutomation

3. **Null Reference Exceptions**
   - Problem: Accessing nullable attributes without checks
   - Solution: Use null-conditional operators (`?.`) and proper null checks
   - Example: `_ac.Attributes?.Temperature` comparisons

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

### State Change Patterns
```csharp
// React to state changes only
sensor.StateChanges().IsOn().Subscribe(TurnOnLight);

// Include current state in initial subscription
sensor.StateChangesWithCurrent().Where(s => s.IsOn()).Subscribe(Configure);

// Time-based reactions
motion.StateChanges()
    .IsOffForMinutes(10)
    .Subscribe(_ => TurnOffLights());
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