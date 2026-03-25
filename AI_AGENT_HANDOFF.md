# AI Agent Handoff: Ability System Project (Deep Technical Dossier)

Last Updated: 2026-03-24
Repository Root: c:\Unity\Ability-System
Target Reader: External AI agent (Claude, GPT, etc.) continuing implementation/hardening work.

## 1) Executive Summary
This repository contains a data-driven Unity ability framework built around ScriptableObject definitions that create runtime action pipelines.

Design center:
- Compose abilities from action definitions.
- Execute in-order runtime actions through a runner.
- Pass data between actions via typed context payloads.
- Support asynchronous/sustained actions (wait, repeat, signal-driven behavior) with explicit cancel/interrupt aftermath semantics.

Current maturity:
- Core architecture is coherent and usable.
- Re-entrancy hardening is partially completed and documented.
- Typed context migration is active and largely in place.
- Baseline tests exist but are still shallow compared to behavior surface.

## 2) Source-Of-Truth Priority (Important)
When docs and code disagree, trust in this order:
1. Runtime source in `Assets/AbilitySystem/Core/Runtime/*`.
2. Action/effect/targeting runtime source.
3. Focused decision logs (`README.RunnerReentrancy.md`, hardening backlog).
4. General README (`Assets/AbilitySystem/README.md`) last.

Reason:
- General docs can lag behind implementation.
- Some backlog items are already partially fixed in code while still listed as open in backlog text.

## 3) System Architecture Map
Static authoring to runtime execution:
1. `AbilityDefinition` stores authored data: name, cooldown, costs, actions, and cast lifecycle signals.
2. `AbilityInstance` creates runtime costs and cooldown state for one caster-bound ability instance.
3. `AbilityInstance.Cast(...)` creates `AbilityCast` with optional typed initial dependencies.
4. `AbilityCast` builds `AbilityContext` + runtime `IAbilityAction` list from definitions.
5. `AbilityRunner` walks action list and executes each action.
6. Actions mutate/read `AbilityContext` payloads, targets, and runtime signals.

Execution model:
- Single runner, index-based progression via `runner.Next()`.
- Synchronous actions call `Next()` immediately.
- Sustained actions keep control and later resolve via complete/cancel/interrupt paths.

## 4) Core Runtime Contracts And Responsibilities

### 4.1 Core entities
- `AbilityDefinition` (`Core/Definition/AbilityDefinition.cs`): authored blueprint.
- `AbilityInstance` (`Core/Runtime/AbilityInstance.cs`): owns costs, cast list, cooldown id, and lifecycle disposal.
- `AbilityCast` (`Core/Runtime/AbilityCast.cs`): one execution attempt; wraps a runner and exposes cast end events.
- `AbilityRunner` (`Core/Runtime/AbilityRunner.cs`): sequential action executor + control-flow endpoints.
- `AbilityContext` (`Core/Runtime/AbilityContext.cs`): typed data/targets/signals transport between actions.

### 4.2 Caster/resource contracts
- `ICaster`: grant/remove ability interface; gameplay owner abstraction.
- `IResourceBearer`: resource consumption and lookup abstraction for costs/conditions.
- `IResource`: resource shape (`CurrentAmount`, `MaxAmount`, consume checks).

### 4.3 Invariants you should preserve
- Runner progresses only via explicit `Next()` from actions or aftermath logic.
- `AbilityContext` payload keys are `Type`, not arbitrary strings.
- Sub-runners must be callback-detached before cleanup stops to avoid re-entrant event loops.
- Action definitions are factory objects only; runtime behavior belongs in runtime action classes.

## 5) AbilityRunner Semantics (Critical)
`AbilityRunner` supports two stop families:

1. Propagating stops:
- `Cancel()`
- `Interrupt()`

Behavior:
- Calls current `SustainedAction` cancel/interrupt method.
- If action returns true, runner executes `TakeAftermathAction(...)`.
- Aftermath can `Next()`, emit `OnCancelled`, or emit `OnInterrupted`.

2. Silent stops (sub-runner ownership cleanup):
- `StopSilentlyAsCancelled()`
- `StopSilentlyAsInterrupted()`

Behavior:
- Calls child action cancel/interrupt but does not run aftermath and emits no events.

Why this exists:
- Prevent parent/child re-entrancy loops when owner actions clean up spawned child runners.

## 6) Context Model: Typed Blackboard + Runtime Signal Registry
`AbilityContext` contains:
- `Caster` (`ICaster`).
- `Targets` (`List<IAbilityTarget>`).
- `_typedBlackboard` (`Dictionary<Type, object>`).

Key APIs:
- `Set<T>(T value)` stores by `typeof(T)`.
- `TryGet<T>(out T value)` retrieves exact type match.
- `SetTargets(List<IAbilityTarget>)` replaces target list.
- `Fork()` snapshots targets + typed entries into a new context object.
- `SetRuntimeSignal(SignalDefinition, RuntimeSignal)` stores per-cast signal instances.
- `TryGetRuntimeSignal(...)` retrieves per-cast runtime signals.

`RuntimeSignalRegistry`:
- Stored inside context as typed payload.
- Maps `SignalDefinition` object references to runtime signal instances.

Important caveat:
- `Fork()` copies dictionary references shallowly. Value objects are not deep-cloned.

## 7) Action System: Definition To Runtime Mapping

### 7.1 Base abstractions
- `AbilityActionDefinition`: ScriptableObject factory with `CreateRuntimeAction()`.
- `IAbilityAction`: runtime behavior contract.
- `SustainedActionDefinition`: adds interrupt/cancel flags and aftermath choices.
- `SustainedAction`: runtime base class implementing cancel/interrupt capability contract.

### 7.2 Core actions and exact behavior
- `TargetingAction`:
  - Runs strategy, writes full target list into context, advances.
- `ApplyEffectAction`:
  - Applies effect instance per target, aggregates applied/skipped/failed counts into `EffectApplySummary`, warns in debug if none applied.
- `WaitAction` (sustained):
  - Starts coroutine wait; on natural finish writes elapsed status and advances; on cancel/interrupt stops coroutine and writes partial wait status.
- `RepeatAction` (sustained):
  - Ticks sub-action sequences over interval/duration using child runners with `context.Fork()`; tracks active child runners and cleanup mode.
- `ConditionalAction`:
  - Evaluates condition, runs chosen branch in sub-runner, propagates completion/cancel/interrupt to parent accordingly.
- `SpawnProjectileAction`:
  - Reads `ProjectileSpawnPoint`; instantiates projectile; publishes runtime signals for hit/destroy slots if configured; writes projectile hit/destroy payloads to context when events fire.
- `WaitForSignalAction` (sustained):
  - Prefers per-cast runtime signal if present in context, else subscribes to global `SignalBus`; advances once signal arrives.
- `RaiseSignalAction`:
  - Raises global signal bus event.
- `RaiseRuntimeSignalAction`:
  - Raises per-cast runtime signal if context has one for slot.
- `DoOnSignalAction` (sustained):
  - Waits for trigger signal; each trigger spawns child sub-runner on forked context; waits for exit signal, then cleans child runners according to configured aftermath + cleanup mode.

### 7.3 Sub-runner cleanup policy
`SubRunnerCleanupMode` options:
- `RespectChildAftermath`: detach callbacks, then child `Cancel/Interrupt` (propagating).
- `ForceSilentStop`: detach callbacks, then silent stop child.
- `DetachAndLetRun`: detach callbacks and do not stop children.

`SubRunnerSubscriptions` ensures:
- Callback references are tracked and detachable by identity.
- Bulk `UnsubscribeAndApplyAftermath(...)` cleanup is centralized and consistent.

## 8) Signal Model: Global And Per-Cast

### 8.1 Global channel
`SignalBus`:
- Static dictionary `SignalDefinition -> List<Action<AbilityContext>>`.
- Subscribe/unsubscribe/raise APIs.

Risk:
- Static lifetime can leak handlers if not unsubscribed or if scene objects die unexpectedly.

### 8.2 Per-cast channel
`RuntimeSignal`:
- Lightweight event wrapper stored in a cast context.
- Isolates simultaneous casts using same authored `SignalDefinition` slot.

Pattern in codebase:
- Producer action creates and stores runtime signal in context.
- Consumer action first tries runtime signal lookup, falls back to global bus.

## 9) Targeting Subsystem
Definitions:
- `TargetingStrategyDefinition` -> runtime strategy factory.
- `SingleTargetStrategyDefinition` (precision sphere or projectile-hit based).
- `AOECircleTargetingStrategy`.

Runtime strategies:
- `SingleTargetStrategy`:
  - Projectile-hit mode: reads `ProjectileHitData` from context and resolves a single `IAbilityTarget` from hit collider.
  - Point mode: reads `TargetPoint`, physics overlap sphere with precision radius and mask.
- `AOECircleStrategy`:
  - Reads `TargetPoint`, overlap sphere by radius/mask, returns all targetable `IAbilityTarget` components.

Target contracts:
- `IAbilityTarget`: `IsTargetable()`.
- `IDamageable`: `TakeDamage(float, ICaster source = null)`.

## 10) Effects Subsystem

### 10.1 Base effects
- `AbilityEffectDefinition` creates runtime `IAbilityEffect`; identity uses `GetInstanceID()`.
- `IAbilityEffect.ApplyTo(...)` returns `AbilityEffectApplyResult`:
  - `Applied`
  - `SkippedUnsupportedTarget`
  - `Failed`

Concrete effects:
- `DamageEffectDefinition` -> `DamageEffect`: applies direct damage to `IDamageable` targets.

### 10.2 Over-time effects
Core classes:
- `OverTimeEffectDefinition`: duration/tick/stack config + stacking policy.
- `DOTEffectDefinition` -> `DOTEffect` (damage per tick times stacks).
- `OverTimeEffect`: base runtime ticking/stack/duration lifecycle.

Manager and grouping:
- `OverTimeEffectLifetimeManager` (`MonoBehaviour` singleton, DontDestroyOnLoad):
  - Tracks target -> effectTypeId -> effect group.
  - Registers/unregisters expiry handlers.
  - Ticks effects in `FixedUpdate()`.
- `OverTimeEffectGroup`:
  - Collection + selection helpers (newest, oldest, least/most stacks, etc.).

Stacking strategy:
- `StackingPolicyDefinition` -> runtime `IStackingPolicy`.
- `BasicStackingPolicy` supports duration refresh/extend and stack behaviors with optional source-scoping.

Known risk:
- Effect type identity uses `GetInstanceID()` which is runtime-session scoped; persistence/network determinism expectations should be made explicit before expansion.

### 10.3 Status effect lifecycle (explicit)
This is the concrete lifecycle for over-time/status-style effects in current code.

1. Creation:
- Producer action (typically `ApplyEffectAction`) calls `AbilityEffectDefinition.CreateEffect(source)`.
- For DOT/over-time definitions this returns an `OverTimeEffect` subtype instance (for example `DOTEffect`).

2. First apply:
- `IAbilityEffect.ApplyTo(target)` is called.
- `OverTimeEffect.ApplyTo(...)` invokes `RegisterToTarget(target)`.
- Registration delegates to `OverTimeEffectLifetimeManager.Instance.RegisterOverTimeEffect(target, effect)`.

3. Group registration and stacking decision:
- Lifetime manager resolves bucket by:
  - target (`IAbilityTarget` key), then
  - effect type id (`OverTimeEffect.EffectTypeId`).
- Manager invokes runtime stacking policy:
  - `effect.StackingPolicy.HandleStacking(target, newEffect, existingGroup)`.
- Depending on policy flags, the new effect may:
  - be inserted as a new entry,
  - merge stacks into existing entries,
  - refresh/extend durations of existing entries,
  - or not be inserted.

4. Tick ownership:
- `OverTimeEffectLifetimeManager.FixedUpdate()` drives ticking.
- For each target and each effect group, manager calls `group.TickAll(deltaTime, target)`.
- Each `OverTimeEffect.Tick(...)` updates remaining time and applies tick payload when interval conditions are met.

5. Expiration and unregister:
- When an effect reaches expiration, `OverTimeEffect` raises `EffectExpired` once.
- Manager-attached expiry handler invokes `UnregisterOverTimeEffect(target, effect)`.
- Unregister removes effect from group and detaches stored handler reference.

6. Explicit cleanup paths:
- `CleanUpTarget(target)` unregisters all effects for one target.
- `CleanUpTargetEffectType(target, effectTypeId)` unregisters only one effect-type bucket.

### 10.4 Status effect ownership model
Ownership in current implementation:
- Effect instance memory owner: `OverTimeEffectLifetimeManager` (indirect, by registry dictionaries).
- Tick scheduler owner: `OverTimeEffectLifetimeManager` via `FixedUpdate()`.
- Grouping/selection owner: `OverTimeEffectGroup` per target + effect-type bucket.
- Stacking behavior owner: runtime `IStackingPolicy` instance created from definition.
- Source identity owner: `OverTimeEffect.Source` (caster reference attached at creation).
- Expiration subscription owner: lifetime manager (stores per-effect handler in `_overTimeEffectHandlers`).

Non-owners (important):
- `AbilityRunner` does not own active over-time effect lifetime.
- `AbilityCast` end does not auto-dispose existing over-time effects on targets.
- `AbilityContext` carries apply-time data but does not retain/tick effect registries.

## 11) Projectile Subsystem
Runtime classes:
- `Projectile` abstract base with `OnHit` and `OnDestroyed` events.
- `StraightLineProjectile` implementation:
  - Moves in `Update()`.
  - Emits hit payload on trigger with normal estimation.
  - Supports pierce count and destroys on lifetime expiration or terminal hit.

Payloads:
- `ProjectileHitData` (point, normal, collider).
- `ProjectileDestroyData` (point, normal).

Integration point:
- `SpawnProjectileAction` bridges projectile events into ability context + runtime signal slots.

## 12) Resources And Costs
Definitions:
- `ResourceDefinition` abstract.
- `BaseResourceDefinition` -> `BaseResource` runtime.
- `AbilityCostDefinition` -> `AbilityCost` struct.

Runtime:
- `BaseResource` supports max/current/consume, includes regen amount field but no built-in ticking path.
- `AbilityInstance` checks `IResourceBearer.CanConsumeCost(...)` then consumes on cast.

Open architecture gap:
- Regeneration loop is still a backlog P0 item; there is no canonical central tick ownership in this module yet.

## 13) Cooldown Architecture
Components:
- `Cooldown`: mutable duration state with start/force/tick/end event.
- `CooldownManager` singleton:
  - Tracks all cooldowns per caster and active cooldown subset.
  - Updates cooldowns in `Update()`.
  - Subscribes to cooldown ended events to remove from active map.

Cast integration note:
- `AbilityInstance` starts cooldown via `CooldownManager.Instance.StartCooldown(_caster, Id)`.
- Validate registration path for every ability lifecycle path; start call expects cooldown already registered.

Known risk:
- Manager stores keys by `ICaster`; scene reload or destroyed objects can leave stale dictionary entries if cleanup is incomplete.

## 14) Lifecycle Trace (Nominal Cast)
1. Caller requests cast on `AbilityInstance`.
2. Instance validates cooldown and costs.
3. Costs consumed and cooldown start requested.
4. `AbilityCast` created with initial dependencies.
5. `AbilityRunner.Next()` begins first action.
6. Actions mutate context and targets.
7. Sustained actions eventually resolve via `Next`, cancel, or interrupt.
8. Runner emits `OnCompleted`, `OnCancelled`, or `OnInterrupted`.
9. `AbilityCast` forwards event with final context.
10. `AbilityInstance` completion callback path removes callback mapping for that cast.

## 15) Cancellation/Interrupt Semantics Trace

Main runner:
- If current action is not `SustainedAction`, cancel/interrupt has no effect.
- If sustained and action returns false from cancel/interrupt, runner does not apply aftermath.
- If returns true, aftermath drives next behavior (`None`, `Cancel`, `Interrupt`).

Parent/child runners:
- Owner action should avoid direct child `Cancel/Interrupt` calls unless callbacks are detached first.
- Use `SubRunnerSubscriptions` and `SubRunnerCleanupMode` pathways.

## 16) Typed Context Payload Inventory (Confirmed)
Located in `Core/Runtime/ContextData`:
- `TargetPoint`
- `ProjectileSpawnPoint`
- `ProjectileLaunchDirection`
- `RepeatTickCount`
- `WaitStatus`
- `EffectApplySummary`

Observed consumers/producers:
- `TargetPoint`: consumed by `AOECircleStrategy`, `SingleTargetStrategy` (point mode).
- `ProjectileSpawnPoint` and `ProjectileLaunchDirection`: consumed by `SpawnProjectileAction`.
- `ProjectileHitData` and `ProjectileDestroyData`: produced by projectile events, consumed by signal/targeting chains.
- `RepeatTickCount`: produced by `RepeatAction` stop path.
- `WaitStatus`: produced by `WaitAction` natural and stop paths.
- `EffectApplySummary`: produced by `ApplyEffectAction`.

## 17) Known Risks, Bugs, And Drift Areas

1. Resource regeneration runtime path incomplete (P0 backlog).
2. Singleton scene-safety/stale-reference risk in cooldown and over-time managers (P0 backlog).
3. `SignalBus` static subscribers can leak if lifecycle unsubscribe discipline breaks.
4. `AbilityInstance` cast bookkeeping still deserves stress tests for long-running sessions.
5. `GetInstanceID()` identity semantics for effects/signals are session-scoped and should be explicitly accepted or replaced for deterministic contexts.
6. Some doc statements are stale versus implementation (for example backlog item claiming null-guard gaps in cast methods while code now contains guards).

## 18) Test Coverage Status
EditMode tests in `Assets/Tests/AbilitySystemCoreTests.cs` currently verify:
- Runner in-order execution + completion.
- Cancel aftermath emits cancelled event when expected.
- Interrupt with `None` aftermath advances to next action.
- Typed context roundtrip and fork isolation.
- Runtime signal set/get by definition.

Missing high-value tests:
- Multi-cast parallelism with runtime signal isolation.
- Sub-runner cleanup mode matrix behavior under cancel/interrupt storms.
- `AbilityInstance.Dispose()` lifecycle cleanup under active sustained casts.
- Manager stale reference cleanup under scene reload simulation.
- End-to-end projectile signal chains (`spawn -> hit -> wait-for-signal -> branch`).

## 19) Backlog Alignment Notes
Canonical backlog file:
- `Assets/AbilitySystem/README.HardeningBacklog.md`

Use backlog priorities, but verify current code before implementing an item because at least one listed P1 null-hardening item appears already implemented in runtime.

## 20) Safe First Actions For Next AI Session
Do these before code changes:
1. Re-read core files:
   - `AbilityRunner.cs`, `AbilityContext.cs`, `SubRunnerSubscriptions.cs`, `RepeatAction.cs`, `DoOnSignalAction.cs`, `WaitForSignalAction.cs`.
2. Re-read backlog and runner re-entrancy decision log.
3. Validate current diagnostics in IDE to catch stale generated project state.

Then choose one contained task:
- Task A: Add high-value tests (no behavior changes).
- Task B: Implement deterministic resource regen path + tests.
- Task C: Add scene-safe cleanup API and tests for singleton managers.

## 21) Recommended Implementation Order
1. Tests first for current behavior contracts around runner/sub-runner semantics.
2. Resource regen baseline (isolated, deterministic).
3. Manager cleanup hardening.
4. Optional identity semantics cleanup (effect/signal IDs) if needed by product direction.

## 22) Guardrails While Modifying
- Do not alter runner cancel/interrupt aftermath semantics without updating decision log and tests.
- Do not replace typed context with string-key-only logic.
- Avoid broad refactors mixed with behavior changes in one PR.
- Keep per-action responsibilities narrow; compose via pipelines, not monolith actions.
- Preserve `context.Fork()` usage for multi-trigger/multi-tick sub-runner scenarios unless replaced with an equivalent isolation strategy.

## 23) Quick File Index
Core definitions:
- `Assets/AbilitySystem/Core/Definition/AbilityDefinition.cs`
- `Assets/AbilitySystem/Core/Definition/AbilityActionDefinition.cs`
- `Assets/AbilitySystem/Core/Definition/AbilityActions/*`
- `Assets/AbilitySystem/Core/Definition/ConditionDefinition.cs`

Core runtime:
- `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityCast.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityRunner.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityContext.cs`
- `Assets/AbilitySystem/Core/Runtime/SubRunnerSubscriptions.cs`
- `Assets/AbilitySystem/Core/Runtime/SubRunnerCleanupMode.cs`

Actions runtime:
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/ApplyEffectAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/TargetingAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/WaitAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/ConditionalAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/SpawnProjectileAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/DoOnSignalAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/WaitForSignalAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/RaiseSignalAction.cs`
- `Assets/AbilitySystem/Core/Runtime/AbilityActions/RaiseRuntimeSignalAction.cs`

Signals/cooldowns:
- `Assets/AbilitySystem/Core/Runtime/Signals/SignalBus.cs`
- `Assets/AbilitySystem/Core/Runtime/Signals/RuntimeSignal.cs`
- `Assets/AbilitySystem/Core/Runtime/Cooldown/Cooldown.cs`
- `Assets/AbilitySystem/Core/Runtime/Cooldown/CooldownManager.cs`

Effects:
- `Assets/AbilitySystem/Effects/Definition/*`
- `Assets/AbilitySystem/Effects/Runtime/IAbilityEffect.cs`
- `Assets/AbilitySystem/Effects/Runtime/DamageEffect.cs`
- `Assets/AbilitySystem/Effects/Runtime/DOTEffect.cs`
- `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/*`

Targeting:
- `Assets/AbilitySystem/Targeting/Definition/*`
- `Assets/AbilitySystem/Targeting/Runtime/Strategies/*`
- `Assets/AbilitySystem/Targeting/Runtime/Targets/*`

Projectiles/resources:
- `Assets/AbilitySystem/Projectiles/Runtime/*`
- `Assets/AbilitySystem/Resources/Definitions/*`
- `Assets/AbilitySystem/Resources/Runtime/*`

Tests/docs:
- `Assets/Tests/AbilitySystemCoreTests.cs`
- `Assets/Tests/Tests.asmdef`
- `Assets/AbilitySystem/README.HardeningBacklog.md`
- `Assets/AbilitySystem/README.RunnerReentrancy.md`

## 24) Hand-off Note To Next AI
If you are making behavior changes, include in your output:
1. Explicit statement of which runner semantics are affected.
2. Exact list of new/updated tests and what contract each one protects.
3. Any change in context payload shape or signal routing.
4. Whether singleton cleanup behavior changed across scene transitions.

This project is close to stable architecture but not yet fully hardened. Prefer small, test-backed increments.

## 25) Ownership Matrix (Who Owns What)
Use this table as the authoritative ownership snapshot for runtime responsibilities.

| Runtime thing | Owner | Created by | Disposed/Stopped by | Notes |
|---|---|---|---|---|
| Ability definition data | ScriptableObject assets | Editor authoring | Unity asset lifecycle | Authoring-only, reused across casts |
| Ability instance state (`AbilityInstance`) | Caller/player ability container | Gameplay code constructing instances | `AbilityInstance.Dispose()` or owner object teardown | Owns costs, cast list, cooldown id |
| A single cast (`AbilityCast`) | `AbilityInstance` cast path | `AbilityInstance.Cast(...)` | Ends naturally or via cancel/interrupt; still tracked in instance list until cleanup paths run | Wraps one runner/context pair |
| Runner execution (`AbilityRunner`) | `AbilityCast` | `AbilityCast` ctor | Internal completion/cancel/interrupt flow | Drives action sequence only |
| Typed cast context (`AbilityContext`) | `AbilityCast`/runner graph | `AbilityCast` ctor | Eligible for GC after cast graph and references are gone | Forked for sub-runner isolation |
| Child runners in repeat/signal actions | Parent sustained action instance | `RepeatAction` / `DoOnSignalAction` / `ConditionalAction` | Parent cleanup policy via `SubRunnerSubscriptions` + `SubRunnerCleanupMode` | Parent must detach callbacks first |
| Global signal subscribers | Static `SignalBus` dictionary | Any signal action/caller | Explicit unsubscribe only | Static lifetime risk |
| Per-cast runtime signals | `AbilityContext` (runtime signal registry) | Producer actions (for example projectile spawn) | Context lifetime end | Isolates simultaneous casts |
| Cooldown state entries | `CooldownManager` | Registration flow + ability setup | Unregister APIs, caster cleanup, or manager lifecycle | Manager is persistent singleton |
| Over-time effects on targets | `OverTimeEffectLifetimeManager` | Effect apply path | Expiration handlers or explicit cleanup APIs | Manager is persistent singleton |
| Over-time grouping buckets | `OverTimeEffectLifetimeManager` | First register for target/type | Cleanup APIs or target/effect removal | Keyed by target + effectTypeId |
| Projectile GameObject | Unity scene/object system | `SpawnProjectileAction` instantiate | Projectile self-destroy / scene unload | Emits hit/destroy events to action wiring |

Owner-boundary rules:
- A cast owns immediate action execution, not long-lived over-time effects.
- Manager singletons own long-lived registries and therefore also own cleanup risk.
- Parent sustained actions own child runner callback wiring and must detach before forcing stop.
