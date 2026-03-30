# AI Agent Handoff: Ability System Project (Deep Technical Dossier)

Last Updated: 2026-03-30
Repository Root: c:/Unity/Ability-System
Target Reader: External AI agent (Claude, GPT, etc.) continuing implementation or hardening work.

## 1) Executive Summary
This repository contains a data-driven Unity ability framework built on ScriptableObject definitions that construct runtime action pipelines.

Design center:
- Compose abilities from action definitions.
- Execute ordered runtime actions through a runner.
- Share data through typed context payloads and target lists.
- Support sustained actions (wait, repeat, signal-driven logic) with explicit cancel and interrupt aftermath behavior.
- Scale effects through an Attributes/Modifiers stat system with source-aware modifier resolution.

Current maturity:
- Core architecture is coherent and usable.
- Context-first effects refactor is completed.
- Attributes/Modifiers pipeline is active for instant and over-time effects.
- DOT modifier reapply path is implemented and idempotent by design.
- Test coverage remains shallow compared to behavior surface.

Architecture grade (current): 8.5/10.

## 2) Source-Of-Truth Priority (Important)
When docs and code disagree, trust in this order:
1. Runtime source under Assets/AbilitySystem/Core/Runtime and effect runtime folders.
2. Action, effect, over-time, and targeting runtime source.
3. Focused logs and summaries:
   - README.RunnerReentrancy.md
   - README.HardeningBacklog.md
   - ARCHITECTURE_SUMMARY.md
4. General README under Assets/AbilitySystem/README.md.

Reason:
- General docs can lag implementation.
- Backlog tasks can remain listed after partial fixes.

## 3) End-To-End Runtime Flow (Most Important)
Authoring to execution:
1. AbilityDefinition stores authored cast data: cooldown, costs, actions, signals.
2. AbilityInstance binds a definition to a caster and tracks runtime state.
3. AbilityInstance.Cast(...) creates AbilityCast with optional typed dependencies.
4. AbilityCast builds AbilityContext and runtime action list from definitions.
5. AbilityRunner executes actions in order using explicit Next() progression.
6. ApplyEffectAction creates runtime effect instances using context-first construction.
7. Effect resolves modifiers, computes snapshot offensive values, and applies to targets.
8. Target.TakeDamage(...) applies dynamic defensive logic at impact time.

Detailed effect path:
1. ApplyEffectAction loops resolved targets.
2. For each target:
   - effect = effectDefinition.CreateEffect(context)
   - result = effect.ApplyTo(target)
3. Effect.ApplyTo(target):
   - resolves modifier source attributes from caster or target
   - binds source attribute into each modifier
   - applies modifiers to local damage/heal attribute object
   - sends computed runtime value to target contract

Key model decision:
- Snapshot offense, dynamic defense.
- Offensive scaler inputs are captured at effect apply time.
- Defense remains live in target logic per hit or tick.

## 4) Core Runtime Contracts And Responsibilities

### 4.1 Core entities
- AbilityDefinition (Core/Definition/AbilityDefinition.cs): authored blueprint.
- AbilityInstance (Core/Runtime/AbilityInstance.cs): owns costs, cooldown identity, cast list, lifecycle cleanup.
- AbilityCast (Core/Runtime/AbilityCast.cs): one execution attempt, wraps runner, exposes completion events.
- AbilityRunner (Core/Runtime/AbilityRunner.cs): sequential executor and stop semantics owner.
- AbilityContext (Core/Runtime/AbilityContext.cs): typed payload, caster, targets, signal registry.

### 4.2 Gameplay contracts
- ICaster: gameplay owner abstraction for ability ownership and source attribution.
- IAbilityTarget: target contract for targeting and effect application boundaries.
- IDamageable: damage intake boundary with optional caster source.
- IAttributeHolder: attribute access contract for modifier source lookups and cost checks.

### 4.3 Invariants to preserve
- Runner progression happens only via explicit Next() or aftermath.
- Context payload keys are Type-based in typed blackboard APIs.
- Sub-runner callbacks must be detached before owner cleanup stop calls.
- Definitions are factories only; behavior lives in runtime classes.
- Effect instances are per-application and do not share mutable modifier binding state globally.

## 5) AbilityRunner Semantics (Critical)
Stop families:

1. Propagating stops:
- Cancel()
- Interrupt()

Behavior:
- If current action is SustainedAction, runner calls action cancel or interrupt path.
- If action returns true, runner executes configured aftermath handling.
- Aftermath can:
  - continue with Next()
  - emit OnCancelled
  - emit OnInterrupted

2. Silent stops (owner cleanup):
- StopSilentlyAsCancelled()
- StopSilentlyAsInterrupted()

Behavior:
- Calls child cancel or interrupt path without aftermath and without parent-facing terminal events.

Why:
- Avoid parent-child re-entrancy loops while cleaning up spawned sub-runners.

## 6) Context Model: Typed Blackboard + Runtime Signal Registry
AbilityContext contains:
- Caster (ICaster)
- Targets (List<IAbilityTarget>)
- typed blackboard (Dictionary<Type, object>)
- runtime signal registry payload

Key APIs:
- Set<T>(T value)
- TryGet<T>(out T value)
- SetTargets(List<IAbilityTarget>)
- Fork()
- SetRuntimeSignal(SignalDefinition, RuntimeSignal)
- TryGetRuntimeSignal(...)

Fork caveat:
- Fork performs shallow copy for payload dictionary values.
- Reference-type payloads remain shared unless caller deep-copies intentionally.

Why this matters for agents:
- Sub-runners built from forked contexts can still observe shared reference payload mutations.
- Keep mutable payload objects minimal or intentionally immutable.

## 7) Action System: Definition To Runtime Mapping

### 7.1 Base abstractions
- AbilityActionDefinition: ScriptableObject factory.
- IAbilityAction: runtime execution contract.
- SustainedActionDefinition: cancel and interrupt capability plus aftermath config.
- SustainedAction: runtime base implementing stop capability contract.

### 7.2 Core actions and behavior summary
- TargetingAction:
  - runs strategy, writes full target list to context, advances.
- ApplyEffectAction:
  - creates effects with CreateEffect(context), applies per target, writes EffectApplySummary.
- WaitAction (sustained):
  - coroutine-based wait with natural and stop-state outputs.
- RepeatAction (sustained):
  - ticks child sequences over interval and duration using context forks.
- ConditionalAction:
  - evaluates condition, runs branch runner, propagates terminal outcome.
- SpawnProjectileAction:
  - instantiates projectile and writes hit or destroy payloads or signals.
- WaitForSignalAction (sustained):
  - prefers runtime signal in context; falls back to global SignalBus.
- RaiseSignalAction:
  - raises global signal event.
- RaiseRuntimeSignalAction:
  - raises context-scoped runtime signal for the slot.
- DoOnSignalAction (sustained):
  - spawns sub-runners on trigger signal and exits on configured exit signal.

### 7.3 Sub-runner cleanup policy
SubRunnerCleanupMode:
- RespectChildAftermath
- ForceSilentStop
- DetachAndLetRun

SubRunnerSubscriptions centralizes callback tracking and detachment.

## 8) Attributes/Modifiers Architecture (Current Refactor)

### 8.1 Runtime stat types
Attribute:
- Fields:
  - BaseValue
  - RuntimeValue
  - modifiers list
- Behavior:
  - RecalculateRuntimeValues applies all modifiers in priority order.
  - Supports AddModifier, AddModifiers, ClearModifiers.
  - Emits OnRuntimeValueChanged and OnBaseValueChanged.

ConsumableAttribute (extends Attribute):
- Adds CurrentAmount.
- Intended contract: 0 <= CurrentAmount <= RuntimeValue.
- Used for health, mana, and similar consumables.
- Emits OnCurrentAmountChanged.

### 8.2 Definition layer
AttributeDefinition:
- ScriptableObject factory for runtime Attribute.

ConsumableAttributeDefinition:
- ScriptableObject factory for ConsumableAttribute with max and initial current values.

AttributeModifierDefinition:
- ScriptableObject factory for runtime AttributeModifier.
- Config:
  - priority
  - source (Caster or Target)
  - attributeName (string lookup key)
  - percent input in inspector (0 to 100, converted to 0.0 to 1.0)
  - strategy (Base, Runtime, Current)

### 8.3 Modifier runtime behavior
AttributeModifier:
- Source-aware binding to a bonus attribute at apply time.
- Strategy behavior:
  - Base: bonusAttribute.BaseValue * percent
  - Runtime: bonusAttribute.RuntimeValue * percent
  - Current: consumable.CurrentAmount * percent
- Contribution is added into target attribute runtime composition.

### 8.4 Source resolution model
For each modifier on an effect:
1. Inspect source enum.
2. Resolve IAttributeHolder:
   - Caster source: effect context caster
   - Target source: current target
3. Lookup source attribute by name.
4. Bind source attribute into modifier instance.
5. Add modifier to local effect attribute and recalc runtime value.

Why this model is safe:
- Resolution happens per effect instance and target application.
- No shared bound attribute references across different cast applications.
- Shared `ModifierResolutionHelper` is now the canonical implementation used by DamageEffect and DOTEffect.

### 8.5 Attribute validator contract (editor asset scan)
Intent:
- Validator is for warnings and suggestions only.
- Runtime attribute lookup remains exact-name by design.

Execution model:
1. Validation scans authored assets (AttributeDefinition and AttributeModifierDefinition) in editor.
2. Validation does not depend on scene load order or runtime registration state.
3. Run via menu command: Ability System/Validation/Validate Attribute References.

Why:
- Deterministic validation independent of runtime object lifetime.
- Easy to run during authoring, pre-build, or CI.
- Keeps runtime stat classes focused on gameplay behavior rather than validator bookkeeping.

## 9) Effect System: Context-First Construction

### 9.1 Contract change
IAbilityEffect apply signature is simplified to ApplyTo(target).
AbilityContext is provided at effect creation through effect definition factory.

Current pattern:
- effect = definition.CreateEffect(context)
- effect.ApplyTo(target)

Why this matters:
- Effects can read caster, level, metadata, and typed payloads at construction.
- Apply call remains focused on target execution.

### 9.2 DamageEffect behavior
DamageEffect runtime behavior:
1. receives context and runtime modifier list at construction.
2. on ApplyTo(target), resolves and binds all modifiers.
3. computes final damage from local Attribute runtime value.
4. sends damage to IDamageable.TakeDamage(finalDamage, context.Caster).

Usage caveat:
- DamageEffect runtime instances are intended for single apply usage in pipeline flow.
- Reusing the same runtime instance can compound modifiers across multiple ApplyTo calls.
- Normal pipeline behavior creates fresh runtime effect instances per application.

### 9.3 Over-time effects and DOT behavior
OverTimeEffect base:
- owns duration, tick cadence, stacks, source context.
- exposes ApplyModifiers(target) extension point.

DOTEffect:
- stores per-instance _damagePerTick attribute.
- ApplyModifiers(target) mirrors DamageEffect source-resolution logic.
- ClearModifiers before re-add prevents duplicate stacking on reapply calls.
- tick sends _damagePerTick.RuntimeValue * stacks to target damage contract.

## 10) Over-Time Lifetime Ownership And Reapply Flow
Ownership:
- OverTimeEffectLifetimeManager owns active effect tracking and tick scheduling.
- Grouping key is target + effect type identity.
- OverTimeEffectGroup manages same-type collection behavior.

Lifecycle:
1. effect ApplyTo(target) delegates registration to manager.
2. manager resolves target and effect-type bucket.
3. stacking policy decides insert, merge, refresh, or reject.
4. FixedUpdate ticks groups and effects and runs periodic cleanup.
5. expiry callback unregisters and detaches handlers.

Cleanup and dictionary pruning (current hardening):
- FixedUpdate runs `PruneEmptyEntries()` every 0.5 seconds (configurable via `_pruneIntervalSeconds`).
- `UnregisterOverTimeEffect()` and `CleanUpTargetEffectType()` call `PruneTargetEntries(target)` immediately.
- Prune methods remove empty effect-type groups and target buckets to prevent unbounded Dictionary growth.
- Long-running sessions no longer accumulate stale entries over time.

Reapply flow (current hardening):
1. trigger stat state change on source or target.
2. call OverTimeEffectLifetimeManager.ReApplyOverTimeEffectsModifier(target[, effectTypeId]).
3. manager iterates active effects.
4. each DOT clears old modifiers and rebinds based on current attributes.
5. damage-per-tick runtime value is recomputed safely.

Idempotency note:
- repeated reapply calls should not accumulate duplicate modifiers due to clear-then-add design.

## 11) Signal Model: Global And Per-Cast

Global channel:
- SignalBus static mapping SignalDefinition to subscriber list.
- Useful for broad signaling, but requires unsubscribe discipline.

Per-cast channel:
- RuntimeSignal stored in context registry.
- isolates simultaneous casts using same authored signal definitions.

Producer-consumer pattern:
- producer action creates runtime signal and stores it in context.
- consumer action first attempts runtime signal lookup and falls back to global bus.

## 12) Targeting And Projectile Integration
Targeting strategies:
- Single target via point precision or projectile hit payload.
- AOE circle via overlap sphere and masks.

Projectile integration:
- SpawnProjectileAction publishes projectile hit and destroy payloads.
- downstream actions can consume payloads for targeting, branching, or signaling.

## 13) Cost, Resource, And Attribute Notes (Post-Refactor)
Legacy ability cost wrappers were removed in favor of attribute-based cost flow.

Current direction:
- Attribute and consumable attributes are the primary stat and cost model.
- Actors implement IAttributeHolder for stat retrieval and consumption checks.

Agent caution:
- older docs or assets may still reference removed Resource-era naming.
- verify implementation in runtime code before applying old backlog assumptions.

## 14) Lifecycle Trace (Nominal Cast)
1. caller requests cast on AbilityInstance.
2. instance validates cooldown and cost constraints.
3. cost consumption and cooldown start happen.
4. AbilityCast created with initial dependencies.
5. runner starts action pipeline with Next().
6. actions mutate context and target lists.
7. effect actions create context-bound effect instances and apply to targets.
8. sustained actions resolve by complete, cancel, or interrupt paths.
9. runner emits terminal state.
10. cast forwards context and terminal event to listeners.

## 15) Cancellation And Interrupt Trace
Main runner:
- non-sustained current action: cancel or interrupt is effectively no-op.
- sustained current action returning false on stop: no aftermath.
- sustained returning true: configured aftermath path executes.

Parent-child runner safety:
- detach child callbacks before any owner-initiated stop operation.
- use SubRunnerSubscriptions helper and cleanup mode configuration.

## 16) Typed Context Payload Inventory (Confirmed)
Context payload types currently documented in runtime usage include:
- TargetPoint
- ProjectileSpawnPoint
- ProjectileLaunchDirection
- RepeatTickCount
- WaitStatus
- EffectApplySummary
- ProjectileHitData
- ProjectileDestroyData

Producer-consumer examples:
- TargetPoint produced by setup actions and consumed by targeting strategies.
- EffectApplySummary produced by ApplyEffectAction.
- projectile payloads produced by projectile events and consumed by downstream actions.

## 17) Known Risks And Drift Areas
1. String attribute names are typo-prone; mitigated by editor asset-based validator (run via menu or CI).  
2. ✅ ~~Modifier source resolution logic is duplicated between DamageEffect and DOTEffect.~~ Extracted to `ModifierResolutionHelper`.
3. ✅ Test coverage for modifier composition and reapply behavior expanded (priority ordering, DOT idempotency, isolation, single-instance contract documented).
4. Static SignalBus can leak handlers if unsubscribe discipline breaks.
5. ✅ ~~Manager dictionaries may retain stale scene references without robust cleanup paths.~~ Mitigated by periodic prune (0.5s interval) + immediate cleanup on unregister.

## 18) Testing Status And Gaps
Current tests cover baseline runner/context behavior and core stat pipeline behavior.

High-value missing tests:
1. full integration: caster stat scaling -> effect snapshot -> target mitigation.
2. multi-target and large-scale stress scenarios.
3. broader lifecycle cleanup tests around scene transitions.

Covered now:
- modifier priority ordering composition.
- DOT reapply idempotency under repeated calls.
- per-caster effect instance isolation and explicit single-instance compounding behavior.

## 19) Backlog Alignment Notes
Primary hardening list is in Assets/AbilitySystem/README.HardeningBacklog.md.

Current priority direction:
1. P1 attribute-name validator at startup.
2. P1 shared helper extraction for modifier source resolution.
3. P2 unit and integration tests for modifiers and reapply flow.

## 20) Safe First Actions For Next AI Session
Before changes:
1. reread runtime contracts:
   - AbilityRunner.cs
   - AbilityContext.cs
   - ApplyEffectAction.cs
   - DamageEffect.cs
   - DOTEffect.cs
   - OverTimeEffectLifetimeManager.cs
2. reread ARCHITECTURE_SUMMARY.md and hardening backlog.
3. validate diagnostics in IDE before implementing.

Suggested contained tasks:
1. add tests for modifier ordering and reapply idempotency.
2. implement attribute-name startup validator.
3. extract shared modifier-resolution helper used by DamageEffect and DOTEffect.

## 21) Recommended Implementation Order
1. Tests first for current modifier and reapply contracts.
2. Startup validator for string attribute name hardening.
3. Modifier resolution helper extraction.
4. Additional manager lifecycle cleanup and signal discipline checks.

## 22) Practical Agent Notes
- Prefer non-destructive, scoped changes and verify against current runtime code.
- Avoid trusting stale assumptions from pre-2026-03-29 docs.
- Preserve snapshot offense and dynamic defense semantics unless explicitly redesigning combat behavior.
- If changing effect signatures, verify all definition factory and action call sites.
- For DOT changes, always preserve clear-before-readd modifier behavior on reapply.