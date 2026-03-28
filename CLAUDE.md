# CLAUDE.md

Agent reference for the HomeAutomation repository — a **NetDaemon v5** (.NET 9 / C# 13) home automation app running on Home Assistant.

- [NetDaemon docs](https://netdaemon.xyz/) · [GitHub](https://github.com/net-daemon/netdaemon)
- **Coding standards & naming conventions →** [CODING_GUIDELINES.md](./CODING_GUIDELINES.md)

---

## Golden-Path Commands

```bash
dotnet build                        # Build
dotnet test                         # Run all tests
nd-codegen                          # Regenerate HA entities → HomeAssistantGenerated.cs
dotnet csharpier format .           # Format (CSharpier)
dotnet csharpier check .            # Check formatting
dotnet format <project> --verify-no-changes   # Verify analyzer / editorconfig issues
.\publish.ps1                       # Deploy to Home Assistant add-on
.\test-coverage.ps1                 # Interactive test + coverage workflow
```

> Coverage scope: only `src/apps/**` (automation logic). 80 % threshold.

---

## Architecture (TL;DR)

```
/apps
  /Area          ← Room automations (Bathroom, Bedroom, Kitchen, …)
  /Common
    /Base        ← AutomationBase → MotionAutomationBase, FanAutomationBase
    /Contracts   ← Truly shared contracts only
    /Devices     ← Shared explicit facades like GlobalDevices
    /Interface   ← IAutomation
    /Services    ← DimmingLightController, HaEventHandler (composition)
    /Security    ← Shared people / security contracts
  /Security      ← Locks, location, notifications
  /Helpers       ← Constants (HaIdentity, HaEntityStates), TimeRange
```

### Feature-Local Entity Contracts

Keep feature-specific entity contracts and mappers beside the consuming automation or device. Only keep truly cross-cutting contracts in `Common`.

```
Area/Bathroom/Automations/IBathroomLightEntities.cs
Area/Bathroom/Automations/BathroomLightEntities.cs
Area/LivingRoom/Devices/ITclDisplayEntities.cs
Area/LivingRoom/Devices/TclDisplayEntities.cs
Common/Contracts/AutomationEntityInterfaces.cs   # IMotionBase, ILightAutomationEntities, IFanAutomationEntities
Common/Devices/GlobalDevices.cs                  # shared whole-home entities
```

- Prefer explicit device facades like `BathroomDevices`, `KitchenDevices`, `LivingRoomDevices`, `BedroomDevices`, `DeskDevices`, `SecurityDevices`, and `GlobalDevices`.
- Avoid reintroducing generic `Common/Containers` catch-all files or nullable area capability bags.
- Cross-area reads should use small collaborator contracts or explicit facade properties, not a global house graph.

### Composition over Inheritance

Complex behaviours (dimming, event handling) live in **service classes** injected via composition, not extra base classes.

- `DimmingLightController` replaces the old `DimmingMotionAutomationBase`.
- `HaEventHandler` wraps `IHaContext.Events`.

### Automation Lifecycle

1. Constructor (DI only — **never block**)
2. `StartAutomation()` → calls `GetSwitchableAutomations()`
3. Master switch toggles subscriptions on/off
4. `Dispose()` → cleans up via `CompositeDisposable`

---

## Critical Rules

### 1. Subscription Memory Safety

Every Rx subscription **must** be returned from `GetSwitchableAutomations()` (or explicitly tracked and disposed). Untracked subscriptions = **memory leaks**.

```csharp
// ✅ Correct — tracked by base class
protected override IEnumerable<IDisposable> GetSwitchableAutomations()
{
    yield return sensor.StateChanges().Subscribe(HandleChange);
}

// ❌ Wrong — never disposed
public override void StartAutomation()
{
    sensor.StateChanges().Subscribe(HandleChange);
}
```

### 2. Error Handling in Subscriptions

An unhandled exception inside `Subscribe()` **terminates the stream**. Always use `SubscribeSafe()` or a try-catch wrapper.

```csharp
// ✅ 
sensor.StateChanges()
    .SubscribeSafe(OnStateChanged, ex =>
        Logger.LogError(ex, "Subscription error in {Class}", nameof(MyAutomation)));

// ❌ 
sensor.StateChanges().Subscribe(OnStateChanged); // one throw = dead stream
```

### 3. No Raw Async Lambdas in Subscribe

Wrap with try-catch + CancellationToken; never fire-and-forget async inside `Subscribe()`.

---

## Key Helpers & Extensions

| Helper / Extension | Purpose |
|---|---|
| `IsOn()` / `IsOff()` | Filter state changes |
| `IsOnForSeconds(n)` / `IsOnForMinutes(n)` | Time-delayed state filter |
| `WhenStateIsFor(pred, ts)` | Generic time-based filter |
| `BinarySensorEntity.IsOccupied()` | Occupancy check |
| `HaIdentity.IsManuallyOperated(userId)` | Manual vs system action |
| `TimeRange.IsCurrentTimeInBetween(h1, h2)` | Hour-range check |

---

## Quick Collection Patterns

| When | Use |
|---|---|
| Fixed small list of entities | `[] ` collection expression |
| Combining sources | `[..a, ..b]` spread |
| Subscriptions / complex logic | `yield return` |
| Complex conditionals | tuple `switch` expression |

---

## Project Config Notes

- **Generated code**: `HomeAssistantGenerated.cs` (run `nd-codegen` to refresh)
- **Global usings**: `apps/GlobalUsings.cs` — add when ≥ 3 files share a namespace
- **EditorConfig**: `.editorconfig` enforces style rules
- **Final newline preference**: keep `insert_final_newline = true`
- **HA connection**: `appsettings.json` → `homeassistant.local:8123`
- **Dev token**: `appsettings.Development.json`

---

## Formatting & Diagnostics Preferences

- **Primary formatter**: use `dotnet csharpier format .` for broad formatting changes.
- **Verification**: use `dotnet csharpier check .` and `dotnet format <project> --verify-no-changes` to confirm formatter / analyzer cleanliness.
- **Targeted fixes first**: if only imports or whitespace are failing, prefer `dotnet format --diagnostics IMPORTS WHITESPACE --include ...` over repo-wide churn.
- **EditorConfig first**: if `dotnet format` suddenly reports many `FINALNEWLINE` diagnostics, inspect `.editorconfig` before mass-editing files.
- **VSCode diagnostics**: if CLI build/test/format checks are clean but VSCode still shows problems, reload the C# language server or window before changing code.
- **Transient compiler locks**: `CS2012` / `VBCSCompiler` file-lock errors can happen when build/test overlap; rerun sequentially before treating them as code issues.
