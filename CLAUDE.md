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

### Code Formatting
```bash
# Format all C# files with CSharpier
dotnet.exe csharpier format .

# Check formatting without applying changes
dotnet.exe csharpier check .
```

### Testing & Code Coverage
```bash
# Run all tests
dotnet.exe test

# Run tests with basic code coverage
dotnet.exe test --collect:"XPlat Code Coverage"

# Run tests with coverage using runsettings
dotnet.exe test --settings coverlet.runsettings

# Run tests with coverage and generate HTML report
dotnet.exe test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/*/coverage.cobertura.xml" -targetdir:"./coverage-report" -reporttypes:Html

# Run tests with coverage threshold enforcement (80% minimum)
dotnet.exe test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80

# Generate detailed coverage report with multiple formats
dotnet.exe test /p:CollectCoverage=true /p:CoverletOutputFormat="json,cobertura,opencover,lcov" /p:CoverletOutput="./coverage/"
reportgenerator -reports:"./coverage/coverage.cobertura.xml" -targetdir:"./coverage-report" -reporttypes:"Html,HtmlSummary,TextSummary"

# Quick coverage check with console output
dotnet.exe test /p:CollectCoverage=true /p:CoverletOutputFormat=console

# Use the convenient test-coverage script (recommended)
.\test-coverage.ps1
```

### Test & Coverage Scripts
Two convenient scripts are available for testing workflows:

#### PowerShell Script (Recommended)
The PowerShell script provides the best experience with advanced error handling, colored output, and cross-platform support:

```powershell
# Interactive menu
.\test-coverage.ps1

# Direct commands
.\test-coverage.ps1 -Action test        # Run tests only
.\test-coverage.ps1 -Action coverage    # Run tests with coverage
.\test-coverage.ps1 -Action report      # Run tests + coverage + HTML report
.\test-coverage.ps1 -Action all         # Full workflow + open browser
.\test-coverage.ps1 -Action clean       # Clean coverage artifacts
```

#### Batch Script Alternative
For environments with restricted PowerShell execution policies:
```cmd
# Interactive menu
test-coverage.bat

# Direct commands
test-coverage.bat test        # Run tests only
test-coverage.bat coverage    # Run tests with coverage
test-coverage.bat report      # Run tests + coverage + HTML report
test-coverage.bat all         # Full workflow + open browser
test-coverage.bat clean       # Clean coverage artifacts
```

#### PowerShell Execution Policy Setup
If you get "execution policy" errors, run one of these commands as Administrator:

```powershell
# Option 1: Allow current user to run scripts (recommended)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Option 2: Temporary bypass for current session only
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Option 3: Run without changing policy
powershell -ExecutionPolicy Bypass -File .\test-coverage.ps1
```

### Troubleshooting

#### PowerShell Execution Policy Issues
```powershell
# Error: "execution of scripts is disabled on this system"
# Solution 1: Set policy for current user (permanent)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Solution 2: Bypass policy for single execution
powershell -ExecutionPolicy Bypass -File .\test-coverage.ps1

# Solution 3: Use the batch script alternative
test-coverage.bat
```

#### Missing Tools
```bash
# Install ReportGenerator if missing
dotnet tool install -g dotnet-reportgenerator-globaltool

# Update ReportGenerator
dotnet tool update -g dotnet-reportgenerator-globaltool

# Check installed tools
dotnet tool list -g
```

#### Coverage Scope
Code coverage is configured to analyze **only the `src/apps` directory**, which contains your NetDaemon automation business logic. This focused approach:
- **Excludes infrastructure code** - Generated files, program.cs, Startup.cs are not analyzed
- **Focuses on automation logic** - Only your area automations, base classes, and helpers count toward coverage
- **Provides meaningful metrics** - 80% threshold applies to code you actively maintain

Files included in coverage:
- `src/apps/Area/` - All area-specific automations (Bathroom, Bedroom, Kitchen, etc.)
- `src/apps/Common/` - Base classes, interfaces, services, and containers
- `src/apps/Security/` - Security-related automations
- `src/apps/Helpers/` - Utility functions and constants

#### Coverage Threshold Failures
If tests fail due to coverage being below 80%:
- Add more unit tests to increase coverage for automation logic
- Review the HTML report to see which automation classes/methods need testing
- Focus on testing business logic in Area automations and Common base classes
- Temporarily lower the threshold in `HomeAutomation.Tests.csproj` if needed

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
  - **`/Common`** - Base classes, interfaces, and entity containers
    - **`/Base`** - Abstract base classes for automation patterns
    - **`/Containers`** - Entity container interfaces and implementations
    - **`/Interface`** - Core automation interfaces
    - **`/Services`** - Shared service classes (DimmingLightController, HaEventHandler)
  - **`/Security`** - Security-related automations (locks, location, notifications)
  - **`/Helpers`** - Constants and utility functions

### Key Base Classes
- **`AutomationBase`** - Abstract base for all automations with master switch support
- **`MotionAutomationBase`** - Base for motion-triggered automations
- **`FanAutomationBase`** - Base for fan control automations
- **`IAutomation`** - Interface all automations must implement

### Entity Container Architecture
The project uses **Entity Container Pattern** for clean dependency injection and testability:

- **Entity Containers** - Interfaces that group related entities (e.g., `IMotionAutomationEntities`)
- **Specialized Containers** - Area-specific extensions (e.g., `IKitchenMotionEntities : IMotionAutomationEntities`)
- **Shared Entity Containers** - For cross-cutting entity dependencies (e.g., `ILivingRoomSharedEntities`)
- **Service Composition** - Replaces inheritance with composition (e.g., `DimmingLightController`)

### Project-Specific Architecture Hierarchy
```
IAutomation (interface)
    └── AutomationBase (abstract base class)
        ├── Manages master switch functionality
        ├── Handles automation lifecycle (enable/disable)
        └── Implements IDisposable with CompositeDisposable
            ├── MotionAutomationBase
            │   ├── Adds motion sensor logic
            │   ├── Controls sensor delay settings
            │   └── Manages light-motion relationships
            └── FanAutomationBase
                ├── Adds fan control logic
                ├── Motion-based activation patterns
                └── Multi-fan coordination

Entity Container Pattern (Composition over Inheritance):
IEntityContainer
    ├── IMotionAutomationEntities (base motion entities)
    │   ├── IKitchenMotionEntities (kitchen-specific extensions)
    │   ├── IBedroomMotionEntities (bedroom-specific extensions)
    │   └── ILivingRoomMotionEntities (living room cross-area dependencies)
    ├── IFanAutomationEntities (fan control entities)
    ├── IClimateAutomationEntities (climate control entities)
    └── ILivingRoomSharedEntities (shared entities for multiple automations)

Service Composition:
- DimmingLightController (replaces DimmingMotionAutomationBase)
- HaEventHandler (event handling service)
```

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

## NetDaemon v5 Essentials

### Basic Application Structure
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

### Essential Reactive Patterns
- **State Changes**: Use `StateChanges()` for new state events only
- **Current State**: Use `StateChangesWithCurrent()` when you need current state immediately
- **Time-based Filters**: Use `WhenStateIsFor()` for proper time-delayed reactions
- **Safe Subscriptions**: Use `SubscribeSafe()` to prevent exceptions from breaking subscriptions

### Application Lifecycle (NetDaemon v5)
1. **Instantiating**: Constructor called, use for DI setup only
2. **Async Initialization**: Implement `IAsyncInitializable` for async setup
3. **Running**: App actively processing events
4. **Disposing**: Clean up resources when stopping
5. **Stopped**: App fully cleaned up

**Important**: Never block the constructor - use it only for setting up subscriptions

### Subscription Management Pattern
```csharp
// CORRECT: Subscriptions tracked and disposed via master switch
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

## Project-Specific Helpers & Extensions

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

## Known Project-Specific Issues

### Memory Leak in Subscriptions
**Problem**: Subscriptions created in `StartAutomation()` are never disposed
```csharp
// WRONG: Found in some existing files
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

### Thread Safety in ClimateAutomation
**Issue**: `_isHouseEmpty` field accessed from multiple streams
**Solution**: Uses `volatile` keyword and lock synchronization for thread safety

### Unsafe Async Pattern in DimmingMotionAutomationBase
**Issue**: Direct async lambda in Subscribe() without proper error handling
**Impact**: Can terminate reactive stream on exception

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

## Coding Standards

This project follows strict coding standards for consistency, maintainability, and reliability.

**📋 For comprehensive coding guidelines, implementation patterns, error handling standards, testing requirements, and code review checklists, see [CODING_GUIDELINES.md](./CODING_GUIDELINES.md).**

### Quick Reference
- **All subscriptions MUST have error handling** (`SubscribeSafe()` or try-catch)
- **Resource management**: Implement `IDisposable` properly with `CompositeDisposable`
- **Strategic collection patterns**: Use `[]` for fixed collections, `yield return` for subscriptions, `[..]` for combining
- **Modern C# 13 features**: Switch expressions, collection expressions, global usings
- **Documentation**: XML documentation required for all public classes
- **Thread safety**: Use `volatile`, locks, or `Interlocked` for shared state

### Strategic Collection Patterns (Modern C# 13/.NET 9)

**Use Collection Expressions `[]` when:**
- Small, fixed collections (2-5 items): `GetFans() => [_fan1, _fan2, _fan3]`
- Immediate materialization needed: `string[] modes = [AUTO, LOW, MEDIUM, HIGH]`
- Array/List literals where type is obvious
- Simple combining with spread: `[..source1, ..source2, item3]`

**Use `yield return` when:**
- **Subscription management** (NetDaemon memory safety): `yield return sensor.Subscribe(...)`
- Large sequences that benefit from lazy evaluation
- Complex logic between items
- Memory-sensitive operations

**Proven Examples from Codebase:**
```csharp
// ✅ Collection expression for fixed fans
protected override IEnumerable<SwitchEntity> GetFans() =>
    [_ceilingFan, _standFan, _exhaustFan];

// ✅ Spread syntax for combining automations
protected override IEnumerable<IDisposable> GetSwitchableAutomations() =>
    [.. GetLightAutomations(), .. GetSensorDelayAutomations()];

// ✅ Yield return for subscription management
protected override IEnumerable<IDisposable> GetLightAutomations()
{
    yield return MotionSensor.StateChanges().Subscribe(HandleMotion);
    yield return Light.StateChanges().Subscribe(HandleLight);
}

// ✅ Switch expressions for complex logic
return (occupied, doorOpen, powerSaving, isColdWeather) switch
{
    (false, true, _, true) => setting.NormalTemp,
    (false, true, _, false) => setting.UnoccupiedTemp,
    (true, false, _, _) => setting.ClosedDoorTemp,
    (true, true, true, _) => setting.PowerSavingTemp,
    _ => setting.NormalTemp
};
```

### Global Usings Strategy
- **Centralized in `GlobalUsings.cs`**: System, Collections, Threading, Reactive, NetDaemon
- **Remove redundant usings**: Don't repeat what's already global
- **Add new globals**: When 3+ files use the same namespace

### Critical Requirements
- Never use async lambdas in `Subscribe()` without error handling
- Always return subscriptions from `GetSwitchableAutomations()`
- Include structured logging with appropriate context
- Follow strategic collection patterns ([] vs yield return)
- Use modern C# 13 features appropriately

## Naming Conventions & Standards

This project follows strict naming conventions to ensure consistency, maintainability, and clarity across the entire codebase.

### Field & Variable Naming

**Private Fields:**
```csharp
// ✅ CORRECT: Underscore prefix with camelCase
private readonly SensorEntity _motionSensor;
private volatile bool _isManuallyControlled;
private static readonly HashSet<string> _knownUsers;

// ❌ INCORRECT: Missing underscore prefix
private readonly SensorEntity motionSensor;
private volatile bool IsManuallyControlled;
```

**Public Properties & Local Variables:**
```csharp
// ✅ CORRECT: PascalCase for public, camelCase for local
public string EntityId { get; set; }
public bool IsOccupied() => _sensor.State.IsOn();

private void ProcessData()
{
    var currentState = _sensor.State;  // camelCase local variable
    bool isActive = currentState.IsOn();
}
```

### Constant Naming

**All Constants (Public & Private):**
```csharp
// ✅ CORRECT: UPPER_CASE convention for all constants
public const string CLEAR_NIGHT = "clear-night";
public const string DANIEL_RODRIGUEZ = "7512fc7c361e45879df43f9f0f34fc57";

private const int MONITOR_STARTUP_TIMEOUT_SECONDS = 30;
private const double EXCELLENT_AIR_THRESHOLD = 6.0;

// ❌ AVOID: Mixed naming patterns
private const int MonitorStartupTimeoutSeconds = 30;  // PascalCase
private const double excellentAirThreshold = 6.0;     // camelCase
```

**Single Source of Truth:**
```csharp
// ✅ CORRECT: Use existing constants from HaIdentity
string targetService = userId switch
{
    HaIdentity.DANIEL_RODRIGUEZ => "mobile_app_poco_f4_gt",
    HaIdentity.MIPAD5 => "mobile_app_21051182c",
    _ => null,
};

// ❌ INCORRECT: Duplicate constants
private const string DanielUserId = "7512fc7c361e45879df43f9f0f34fc57";  // Duplicate!
```

### Method & Function Naming Patterns

**Event Handlers:**
```csharp
// ✅ STANDARDIZED PATTERN: OnXxx for direct event handlers
private void OnMotionStateChanged(StateChange e)
private void OnAirPurifierStateChanged(StateChange e)
private void OnDeskPresenceChanged(StateChange e)
```

**Business Logic Processing:**
```csharp
// ✅ STANDARDIZED PATTERN: ProcessXxx for complex business logic
private void ProcessExcellentAirQuality()
private void ProcessPoorAirQuality(double? pm25Value)
private void ProcessTemperatureData(double temperature)
```

**State Management:**
```csharp
// ✅ STANDARDIZED PATTERN: ManageXxx for state coordination
private void ManageStandFanState(bool shouldBeOn, double? pm25Value)
private void ManageScreenBasedOnPresence()
private void ManageClimateSettings()
```

**Setup & Configuration:**
```csharp
// ✅ STANDARDIZED PATTERN: GetXxx for automation setup (consistent with base)
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
private IEnumerable<IDisposable> GetLightAutomations()
private IEnumerable<IDisposable> GetSensorDelayAutomations()

// ✅ STANDARDIZED PATTERN: SetXxx/ConfigureXxx for clear configuration
private void SetAcTemperatureAndMode(int temperature, string hvacMode)
private void ConfigureScheduledSettings()
```

**Conditional Actions:**
```csharp
// ✅ CLEAR PURPOSE: ConditionallyXxx for clarity
private void ConditionallyActivateFan(bool activateFan, int targetTemp)
private void ConditionallyTurnOffDevices()

// ✅ SPECIFIC ACTIONS: Clear verb + specific target
private void ApplyScheduledAcSettings(TimeBlock? timeBlock)
private void LogCurrentAcScheduleSettings()
```

**State Checking:**
```csharp
// ✅ BOOLEAN METHODS: IsXxx, HasXxx, CanXxx patterns
public bool IsOccupied() => _sensor.State.IsOn();
private bool IsMonitorShowingPcInput()
private bool HasValidConfiguration()
private bool CanActivateClimate()
```

### Access Modifier Guidelines

```csharp
// ✅ CORRECT: Public for API, Internal for testing
public class MotionAutomation : MotionAutomationBase
{
    // ✅ Internal for simulation/testing methods
    internal void SimulateShowPcWebhook()
    internal void SimulateShutdownWebhook()

    // ✅ Private for implementation details
    private void ProcessMotionData()
    private void ManageDeviceState()
}
```

### Naming Best Practices

1. **Eliminate Duplication**: Always check for existing constants in `HaIdentity` and `HaEntityStates`
2. **Descriptive Purpose**: Method names should clearly indicate what they do and why
3. **Consistent Patterns**: Follow established patterns for similar operations across classes
4. **Clear Overloads**: When method overloads exist, names should distinguish their purpose
5. **Appropriate Access**: Use `internal` for testing/simulation, `private` for implementation

### Before/After Examples

**Event Handler Standardization:**
```csharp
// ❌ BEFORE: Mixed patterns
private void OnPm25Changed(StateChange e)          // OnXxx pattern
private void HandleExcellentAirQuality()           // HandleXxx pattern
private void HandleStandFan(bool shouldBeOn)       // HandleXxx pattern

// ✅ AFTER: Consistent patterns
private void OnPm25Changed(StateChange e)          // OnXxx for events
private void ProcessExcellentAirQuality()          // ProcessXxx for logic
private void ManageStandFanState(bool shouldBeOn)  // ManageXxx for state
```

**Method Clarity Improvements:**
```csharp
// ❌ BEFORE: Ambiguous overloads
private void ApplyAcSettings(TimeBlock? timeBlock)     // Which settings?
private void ApplyAcSettings(int temperature, string hvacMode)  // Same name!
private void ActivateFan(bool activateFan, int targetTemp)     // Confusing param

// ✅ AFTER: Clear purpose
private void ApplyScheduledAcSettings(TimeBlock? timeBlock)    // Scheduled settings
private void SetAcTemperatureAndMode(int temperature, string hvacMode)  // Specific action
private void ConditionallyActivateFan(bool activateFan, int targetTemp)  // Conditional
```

These naming standards ensure consistency, maintainability, and clarity across the entire HomeAutomation codebase.