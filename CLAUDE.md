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
    /Containers  ← Entity container interfaces (IMotionAutomationEntities, …)
    /Interface   ← IAutomation
    /Services    ← DimmingLightController, HaEventHandler (composition)
  /Security      ← Locks, location, notifications
  /Helpers       ← Constants (HaIdentity, HaEntityStates), TimeRange
```

### Entity Container Pattern

Group HA entities behind an interface → inject one container instead of N entities → easy to mock in tests.

```
IMotionAutomationEntities           # base motion entities
  ├─ IKitchenMotionEntities         # area-specific extensions
  └─ ILivingRoomMotionEntities      # cross-area dependencies
IFanAutomationEntities
IClimateAutomationEntities
ILivingRoomSharedEntities           # shared across multiple automations
```

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
- **HA connection**: `appsettings.json` → `homeassistant.local:8123`
- **Dev token**: `appsettings.Development.json`