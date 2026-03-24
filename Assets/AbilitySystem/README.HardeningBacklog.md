# Ability System Hardening Backlog

Last Updated: 2026-03-24
Purpose: single source of truth for architecture hardening and pre-stats/modifiers blockers.

## Scoring Model
- Priority: P0 (critical), P1 (high), P2 (medium), P3 (low)
- Effort: S (0.5-1 day), M (1-3 days), L (3-5 days)
- Impact: H (high risk reduction), M (moderate), L (low)

## Open Tasks

1. **P0** Resource regeneration does not run
- Effort: M
- Impact: H
- Why:
  - Stats/modifiers for regen have no runtime effect yet.
- Touch points:
  - `Assets/AbilitySystem/Resources/Runtime/IResource.cs`
  - `Assets/AbilitySystem/Resources/Runtime/BaseResource.cs`
  - resource owner update loop(s)
- Acceptance criteria:
  - Resources with regen increase over time up to max.
  - Regen behavior is deterministic and testable.

2. **P0** Scene-safe cleanup for singleton managers
- Effort: M
- Impact: H
- Why:
  - Persistent manager dictionaries can hold stale scene-object references.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/Cooldown/CooldownManager.cs`
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffectLifetimeManager.cs`
- Acceptance criteria:
  - Reloading scene repeatedly does not grow stale entries.
  - No stale-reference logs/errors after scene transitions.

3. **P1** Cast null-hardening final pass
- Effort: S
- Impact: H
- Why:
  - Constructor guards exist, but `AbilityCast` methods can still call `_runner` when null.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/AbilityCast.cs`
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs`
- Acceptance criteria:
  - `Execute`, `Cancel`, and `Interrupt` are safe when cast construction fails.
  - Null `ActionDefinitions` path is guarded with clear logs.

4. **P1** Ability add/remove symmetry (single remove path)
- Effort: S
- Impact: M
- Why:
  - One-off `RemoveAbility(...)` should mirror full `Dispose()` unsubscription behavior.
- Touch points:
  - `Assets/Demo/Player/Player.cs`
- Acceptance criteria:
  - `RemoveAbility(...)` unsubscribes cast complete/cancel/interrupt callbacks for that ability.
  - Re-add/remove cycles do not leak signal subscriptions.

5. **P1** Cast callback bookkeeping cleanup
- Effort: S
- Impact: M
- Why:
  - `AbilityInstance` now tracks callback mappings and needs explicit cleanup guarantees.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs`
- Acceptance criteria:
  - Finished casts are removed from `_casts`.
  - Callback dictionary does not grow over time.

6. **P2** Typed key strategy completion
- Effort: L
- Impact: H
- Why:
  - Resource/context keys are still mostly string-based.
- Touch points:
  - `Assets/AbilitySystem/Core/Definition/ContextKeys.cs`
  - `Assets/AbilitySystem/Core/Runtime/AbilityContext.cs`
  - `Assets/AbilitySystem/Resources/*`
- Acceptance criteria:
  - High-frequency paths use typed keys/refs.
  - String keys are limited to extensibility/escape-hatch cases.

7. **P2** Effect identity semantics decision (`GetInstanceID`)
- Effort: S
- Impact: M
- Why:
  - `GetInstanceID` is session-scoped identity.
- Touch points:
  - `Assets/AbilitySystem/Effects/Definition/AbilityEffectDefinition.cs`
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffect.cs`
- Acceptance criteria:
  - Identity scope is documented and matches runtime expectations.

8. **P3** Architecture test baseline
- Effort: M
- Impact: H
- Why:
  - Core flows still rely mostly on manual verification.
- Touch points:
  - New test assemblies under `Assets/AbilitySystem/Tests/`
- Acceptance criteria:
  - Tests cover: runner outcomes, effect apply outcomes, lifecycle cleanup, and repeat context isolation.

## Recently Fixed

1. [x] AbilityCost compile blocker resolved
- Evidence:
  - `Assets/Demo/Player/Player.cs` now uses `cost.cost`.
  - Workspace diagnostics currently show no errors.

2. [x] Effect apply outcome reporting
- Evidence:
  - `Assets/AbilitySystem/Effects/Runtime/IAbilityEffect.cs` returns `AbilityEffectApplyResult`.
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/ApplyEffectAction.cs` aggregates applied/skipped/failed counts.

3. [x] Repeat action context isolation
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs` creates sub-runners with `context.Fork()`.

4. [x] Demo contract cleanup for enemy target
- Evidence:
  - `Assets/Demo/Player/Enemy.cs` no longer implements `ICaster` and removed `NotImplementedException` stubs.

5. [x] Ability lifecycle disposal baseline
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs` exposes `Dispose()`.
  - `Assets/Demo/Player/Player.cs` disposes abilities in `OnDestroy()`.

6. [x] Naming cleanup for single target strategy definition
- Evidence:
  - Active file is `Assets/AbilitySystem/Targeting/Definition/SingleTargetStrategyDefinition.cs`.

## Suggested Next Session (Short)
1. Add null-safe guards in `AbilityCast.Execute/Cancel/Interrupt`.
2. Finish single-ability unsubscription in `Player.RemoveAbility(...)`.
3. Start resource regen baseline (`IResource` regen API + tick loop).
