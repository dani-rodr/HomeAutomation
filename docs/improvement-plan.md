# Reliability and Async Hardening Plan

Branch: `feature/reliability-async-tdd-hardening`

## Status Legend

- [ ] Pending
- [-] In progress
- [x] Done

## Phase Checklist

### Phase 1 - Async and subscription safety

- [x] Replace raw async Rx subscriptions in Bathroom and LivingRoom light automations.
- [x] Fix untracked nested `Take(1)` subscription lifecycle in `MotionSensorBase`.
- [x] Fix nested disposable list mutation and growth risk in `Laptop` logoff flow.
- [x] Add/adjust tests for async subscription behavior and disposal tracking.

### Phase 2 - Lifecycle startup hardening

- [x] Add rollback/compensation in `AutomationBase.StartAutomation` for partial failures.
- [x] Add rollback/compensation in `AppBase` when startup fails mid-sequence.
- [x] Add tests covering startup failure cleanup behavior.

### Phase 3 - Crash-proofing and deterministic merge behavior

- [x] Guard empty fan configuration in `FanAutomationBase`.
- [x] Resolve duplicate source merge deterministically in `MediaPlayerBase` (extended overrides base).
- [x] Clear webhook subscription registry on dispose in `WebhookServices`.
- [x] Add tests for guard/merge/dispose behavior.

### Phase 4 - Fire-and-forget safety

- [x] Harden `StartupApp` delayed notification path to avoid unobserved exceptions.
- [x] Add test coverage for exception-safe startup scheduling.

### Phase 5 - Verification and cleanup

- [x] Ensure formatting and analyzers are clean for touched files/projects.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## Commit Log

- Pending commit: reliability hardening across async subscriptions, lifecycle rollback, startup safety, and guard tests.
