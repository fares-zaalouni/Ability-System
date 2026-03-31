# Runner Re-entrancy Decision Log

Last Updated: 2026-03-31 (Attributes/Modifiers and over-time policy refactor review: runner reentrancy pattern remains valid.)

## Why this exists
This note documents the re-entrancy decision in the ability pipeline so future changes do not re-introduce event loops.

## Status Note (2026-03-31)
- `OverTimeEffectLifetimeManager.Tick(...)` now uses indexed loops over snapshots to stay safe when over-time effects unregister during ticking.
- This change is independent from runner re-entrancy rules and does not alter `AbilityRunner` aftermath semantics.

The core issue we hit:
- Parent actions (`DoOnSignalAction`, `RepeatAction`, `ConditionalAction`) spawn sub-runners.
- Parent cleanup called `subRunner.Cancel()` / `subRunner.Interrupt()`.
- Those methods raised `OnCancelled` / `OnInterrupted` and bubbled back into parent cleanup.
- Result: re-entrant cancel/interrupt calls, duplicated aftermath handling, and hard-to-debug behavior.

## Final decision
We split runner semantics into two categories and made child cleanup explicit.

### Runner stop semantics

1. Propagating stop (normal control flow)
- `AbilityRunner.Cancel()`
- `AbilityRunner.Interrupt()`

These DO run aftermath logic via `TakeAftermathAction(...)`.
Use when you want cancellation/interrupt to propagate through the pipeline as a gameplay event.

2. Silent stop (owned sub-runner cleanup)
- `AbilityRunner.StopSilentlyAsCancelled()`
- `AbilityRunner.StopSilentlyAsInterrupted()`

These DO NOT run aftermath logic and DO NOT raise runner events.
Use when an owner action is cleaning up child sub-runners.

### Child cleanup mode semantics
- `SubRunnerCleanupMode.RespectChildAftermath`
   - Detach callbacks, then request child `Cancel`/`Interrupt`.
   - Child current action aftermath is honored.
- `SubRunnerCleanupMode.ForceSilentStop`
   - Detach callbacks, then force child silent stop.
   - Child aftermath is intentionally bypassed.
- `SubRunnerCleanupMode.DetachAndLetRun`
   - Detach callbacks and leave child runners alive/disconnected.

Current defaults:
- `DoOnSignalActionDefinition`: `RespectChildAftermath`
- `RepeatActionDefinition`: `RespectChildAftermath`

## Rule of thumb
- If you are stopping YOUR OWN runner as part of gameplay logic: use `Cancel` / `Interrupt`.
- If you are stopping CHILD runners you created: choose a `SubRunnerCleanupMode` intentionally.

## Sub-runner callback management pattern
To avoid orphaned callbacks and duplicated wiring, use `SubRunnerSubscriptions`.

Lifecycle pattern:
1. Create sub-runner.
2. Subscribe callbacks through `SubRunnerSubscriptions.Subscribe(...)`.
3. On any callback path (`completed/cancelled/interrupted`):
   - `Unsubscribe(subRunner)` first
   - remove sub-runner from owner list
   - then propagate to parent if needed
4. During owner cleanup:
   - snapshot list
   - clear owner list
   - call `UnsubscribeAndApplyAftermath(snapshot, requestedAftermath, cleanupMode)`

## Why callback detaching is still needed
Even with silent stops, detaching matters for `None` aftermath or naturally completing child runners.
Without detaching, old callbacks can fire later and call into a parent action that has already finished.

## Where this is applied
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/DoOnSignalAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/ConditionalAction.cs`
- helper: `Assets/AbilitySystem/Core/Runtime/SubRunnerSubscriptions.cs`

## Anti-patterns to avoid
- Calling `Cancel`/`Interrupt` on child runners during owner cleanup.
- Subscribing inline lambdas without storing/detaching ownership.
- Clearing child runner lists before detaching callbacks.

## Quick checklist for new actions with sub-runners
- [ ] Does the action own child runners?
- [ ] Are child callbacks subscribed through `SubRunnerSubscriptions`?
- [ ] Are callbacks detached before child cleanup?
- [ ] Is `SubRunnerCleanupMode` chosen intentionally for this action?
- [ ] Are orphaned callbacks impossible after owner action ends?
