# Ability System Hardening Backlog

Last Updated: 2026-03-18
Purpose: prioritized architecture hardening plan before Stats/Modifiers and broader gameplay integration.

## Scoring Model
- Priority: P0 (critical), P1 (high), P2 (medium), P3 (low)
- Effort: S (0.5-1 day), M (1-3 days), L (3-5 days)
- Impact: H (high risk reduction), M (moderate), L (low)

## Priority Queue

1. **P0** Scene-safe lifecycle cleanup for singleton managers
- Effort: M
- Impact: H
- Why:
  - Persistent managers keep runtime references to scene objects and can retain stale keys after scene reloads.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/Cooldown/CooldownManager.cs`
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffectLifetimeManager.cs`
- Tasks:
  - Add owner-side unregister in `OnDisable`/`OnDestroy` for casters and targets.
  - Add scene-load defensive cleanup path in managers (opt-in full reset or dead-reference pruning).
  - Add idempotent cleanup methods (safe to call multiple times).
- Acceptance criteria:
  - Reloading scene 10 times does not grow cooldown/effect dictionary counts.
  - No logs/errors from stale caster/target references after reload.

## Recently Fixed

1. [x] Ability instance cleanup on destroy
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs` now exposes `Dispose()`.
  - `Dispose()` unregisters cooldown state and cancels active casts.
  - `Assets/Demo/Player/Player.cs` calls ability disposal from `OnDestroy()` and when removing an ability.

2. [x] Cast construction null-hardening
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityCast.cs` now guards against null `AbilityDefinition`.
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs` skips null cost definitions instead of throwing.

3. [x] Effect apply outcome reporting
- Evidence:
  - `Assets/AbilitySystem/Effects/Runtime/IAbilityEffect.cs` now returns `AbilityEffectApplyResult`.
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/ApplyEffectAction.cs` aggregates applied, skipped, and failed counts.
  - `Assets/AbilitySystem/Core/Definition/ContextKeys.cs` stores effect apply summary values in `AbilityContext`.

## Still Open

- Scene-level pruning for `CooldownManager` and `OverTimeEffectLifetimeManager` is still needed.
- `Player` cleanup now covers local ownership, but singleton managers still need dead-reference handling on scene reloads.

2. **P0** Cast construction null-hardening
- Effort: S
- Impact: H
- Why:
  - Null `Costs` or `ActionDefinitions` from older assets can throw during cast creation.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs`
  - `Assets/AbilitySystem/Core/Runtime/AbilityCast.cs`
- Tasks:
  - Guard null lists when building runtime costs/actions.
  - Fail fast with explicit log message when action list is empty/invalid.
  - Ensure cost consume is called only when costs exist and bearer is non-null.
- Acceptance criteria:
  - Legacy assets with empty or null lists do not crash play mode.
  - Invalid assets log one clear error and safely abort cast.

3. **P1** Ability add/remove lifecycle symmetry
- Effort: S
- Impact: H
- Why:
  - Demo grants abilities and registers cooldown/signal handlers but remove path does not fully unregister.
- Touch points:
  - `Assets/Demo/Player/Player.cs`
- Tasks:
  - Track signal callbacks per ability and unsubscribe on remove.
  - Unregister cooldown entry on remove.
  - Add safe no-op if remove called twice.
- Acceptance criteria:
  - Add/remove ability repeatedly does not leak callbacks or cooldown state.
  - Removed ability cannot be interrupted/cancelled via stale cast handle.

4. **P1** RepeatAction context isolation policy
- Effort: M
- Impact: H
- Why:
  - Tick sub-runners currently share one mutable context, risking cross-tick contamination.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs`
  - `Assets/AbilitySystem/Core/Runtime/AbilityContext.cs`
- Tasks:
  - Add explicit policy enum for `RepeatAction`: `SharedContext` vs `ForkPerTick`.
  - Default to `ForkPerTick` for safer behavior.
  - Document performance/behavior tradeoff.
- Acceptance criteria:
  - Concurrent tick pipelines cannot overwrite each other's target/blackboard values under forked mode.
  - Existing behavior remains available via explicit `SharedContext` mode.

5. **P1** Interface completion or contract narrowing in demo
- Effort: S
- Impact: M
- Why:
  - `NotImplementedException` in demo classes makes integration behavior misleading.
- Touch points:
  - `Assets/Demo/Player/Enemy.cs`
  - `Assets/Demo/Player/Player.cs`
- Tasks:
  - Either implement all declared interfaces or remove unneeded interface declarations.
  - Replace throw paths with deterministic behavior/logging in demo context.
- Acceptance criteria:
  - No `NotImplementedException` reachable during normal demo play.
  - Demo classes only implement contracts they actually support.

6. **P2** Typed key strategy for resources and context
- Effort: L
- Impact: H
- Why:
  - String keys (`resourceName`, context key strings) are typo-prone and difficult to audit.
- Touch points:
  - `Assets/AbilitySystem/Core/Definition/ContextKeys.cs`
  - `Assets/AbilitySystem/Core/Runtime/AbilityContext.cs`
  - `Assets/AbilitySystem/Resources/Runtime/Cost.cs`
  - `Assets/AbilitySystem/Resources/Definitions/AbilityCostDefinition.cs`
- Tasks:
  - Introduce typed resource key identity (SO ref or stable GUID-backed key).
  - Add lightweight typed context slots for common high-value entries.
  - Keep string blackboard as escape hatch for experimental actions.
- Acceptance criteria:
  - Resource lookup no longer depends on free-form name strings.
  - High-frequency context values use typed APIs.

7. **P2** Effect identity semantics review (`GetInstanceID`)
- Effort: S
- Impact: M
- Why:
  - `AbilityEffectDefinition.Id => GetInstanceID()` works at runtime in-session, but is not intended as long-term stable identity across sessions.
- Touch points:
  - `Assets/AbilitySystem/Effects/Definition/AbilityEffectDefinition.cs`
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffect.cs`
- Tasks:
  - Decide intended identity scope: session-only vs persistent/stable.
  - If stable is needed, add serialized GUID field with migration fallback.
  - Update any stacking/grouping assumptions tied to effect type identity.
- Acceptance criteria:
  - Identity behavior is documented and matches runtime expectations.
  - No accidental grouping drift after asset duplication/reimport workflows.

8. **P3** Naming and structure consistency cleanup
- Effort: S
- Impact: M
- Why:
  - Typos and naming drift reduce maintainability and trust.
- Touch points:
  - `Assets/AbilitySystem/Effects/Definition/AbiliyEffectDefinition.cs` (legacy typo path)
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/StackingStategies` (folder typo)
  - `Assets/AbilitySystem/Targeting/Definition/SingleTargetStrategyDefininion.cs` (legacy typo path)
- Tasks:
  - Normalize names and file paths.
  - Keep temporary compatibility if Unity asset references are sensitive; migrate gradually.
- Acceptance criteria:
  - No public API/file naming typos remain in active paths.

9. **P3** Architecture test baseline
- Effort: M
- Impact: H
- Why:
  - No automated tests currently protect runner semantics, lifecycle cleanup, and signal isolation.
- Touch points:
  - New test assemblies under `Assets/AbilitySystem/Tests/`.
- Tasks:
  - Add playmode tests for cast completion/cancel/interrupt aftermath.
  - Add tests for runtime signal isolation under concurrent casts.
  - Add scene reload lifecycle test for managers.
- Acceptance criteria:
  - Core architectural regressions are caught by CI/local test run.

## Suggested Execution Plan

### Sprint A (1 week)
- Items: 1, 2, 3, 5
- Goal: eliminate crash/leak-prone behavior in current demo and runtime.

### Sprint B (1 week)
- Items: 4, 7
- Goal: tighten correctness around concurrent pipelines and identity semantics.

### Sprint C (1 week)
- Items: 6, 8, 9
- Goal: improve long-term maintainability and regression confidence.

## Notes
- Keep this file updated as decisions are made (especially item 7 identity scope).
- If Stats/Modifiers start before Sprint A is done, item 1 and item 2 should still be completed first.