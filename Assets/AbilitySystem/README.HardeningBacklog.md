# Ability System Hardening Backlog

Last Updated: 2026-03-31
Purpose: single source of truth for architecture hardening and MVP → production blockers.

## Scoring Model
- Priority: P0 (critical).0 (critical), P1 (high), P2 (medium), P3 (low)
- Effort: S (0.5-1 day), M (1-3 days), L (3-5 days)
- Impact: H (high risk reduction), M (moderate), L (low)

## Open Tasks

1. **P1** Ensure explicit duration policy wiring on over-time effect assets
- Effort: S
- Impact: H
- Why:
  - Runtime fallback defaults exist for resilience, but authored assets should be explicit.
  - Missing `_durationPolicy` references can silently rely on defaults and drift behavior.
- Touch points:
  - `Assets/Demo/Effects/SO/*.asset` (all over-time effect definitions)
  - `Assets/AbilitySystem/Effects/Definition/OverTimeEffects/OverTimeEffectDefinition.cs`
- Acceptance criteria:
  - Every over-time effect asset has explicit stacking + duration policy references.
  - Demo behavior remains unchanged after explicit wiring.

2. **P1** Automate editor attribute validation in CI/pre-build (string-key fragility)
- Effort: S
- Impact: H
- Why:
  - Attribute names are strings (e.g., "ability_power"). Typos fail at runtime.
  - Modifiers reference attributes by name; editor validator exists but should run automatically.
- Touch points:
  - `Assets/AbilitySystem/Editor/Validation/AttributeReferenceAssetValidator.cs`
  - Build pipeline / CI pre-build step
- Acceptance criteria:
  - Validator runs in CI/pre-build and fails or warns on unknown references.
  - Suggest nearest-match for typos in validation output.

3. **P2** Modifier/effect integration test expansion
- Effort: M
- Impact: H
- Why:
  - Core stat scaling and over-time policy matrix now have baseline tests; expand to stress and full cast flows.
  - Priority ordering, policy interaction, and multi-target paths are critical paths.
- Touch points:
  - `Assets/Tests/` or new `Assets/AbilitySystem/Tests/Modifiers/`
- Acceptance criteria:
  - Add stress test: 100-zombie scenario (parallel abilities, shared target).
  - Add multi-target integration path through ApplyEffectAction.
  - Add end-to-end tests for over-time stacking + duration behavior through full ability casts.

4. **P2** Consumable attribute clamping rules
- Effort: S
- Impact: M
- Why:
  - ConsumableAttribute.Add can overshoot max; SetRuntime can violate invariants.
  - Clamping logic is scattered; single entry point would clarify contracts.
- Touch points:
  - `Assets/AbilitySystem/Attributes and Modifiers/Runtime/Attributes/ConsumableAttribute.cs`
- Acceptance criteria:
  - Add clamping at Add() and internal changes.
  - Current is always ≤ RuntimeValue (max).
  - Document contract clearly (zero-based assertions, if desired).

5. **P2** Cast null-hardening final pass
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

6. **P2** Scene-safe cleanup for singleton managers
- Effort: M
- Impact: H
- Why:
  - Persistent manager dictionaries can hold stale scene-object references.
  - DOT lifetime manager in particular can grow stale entries.
- Touch points:
  - `Assets/AbilitySystem/Core/Runtime/Cooldown/CooldownManager.cs`
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffectLifetimeManager.cs`
- Acceptance criteria:
  - Reloading scene repeatedly does not grow stale entries.
  - No stale-reference warnings after scene transitions.

7. **P3** Ability add/remove symmetry (single remove path)
- Effort: S
- Impact: M
- Why:
  - One-off `RemoveAbility(...)` should mirror full `Dispose()` unsubscription behavior.
- Touch points:
  - `Assets/Demo/Player/Player.cs`
- Acceptance criteria:
  - `RemoveAbility(...)` unsubscribes cast complete/cancel/interrupt callbacks for that ability.
  - Re-add/remove cycles do not leak signal subscriptions.

8. **P3** Attribute change event binding framework
- Effort: M
- Impact: L
- Why:
  - `OnRuntimeValueChanged` / `OnBaseValueChanged` events exist but have no built-in listeners yet.
  - UI/animation binding patterns could be documented or templated.
- Touch points:
  - `Assets/AbilitySystem/Attributes and Modifiers/Runtime/Attributes/`
  - (Optional) new example: UIStatDisplay.cs showing event subscription
- Acceptance criteria:
  - Example code demonstrates subscribing to stat changes for UI.
  - Patterns documented (one-time subscribe vs continual listener).

9. **P3** Architecture test baseline
- Effort: M
- Impact: H
- Why:
  - Core flows (effects apply, modifiers compose, DOT reapply) rely mostly on manual verification.
- Touch points:
  - New test assemblies under `Assets/AbilitySystem/Tests/`
- Acceptance criteria:
  - Tests cover: runner outcomes, effect apply outcomes, lifecycle cleanup, and repeat context isolation.
  - Integration test for stat flow: caster.ability_power → damage calc → target receives scaled damage.

## Recently Fixed (This Session & Prior)

1. [x] **Context-first effects architecture** (2026-03-29)
- Evidence:
  - `IAbilityEffect.ApplyTo(target)` no longer takes context param.
  - `AbilityEffectDefinition.CreateEffect(AbilityContext context)` passes full context at construction.
  - DamageEffect + DOTEffect receive context; can access metadata/level/custom payloads.

2. [x] **Attributes/Modifiers system launch** (2026-03-29)
- Evidence:
  - `Assets/AbilitySystem/Attributes and Modifiers/` complete with Attribute, ConsumableAttribute, AttributeModifier.
  - `ModifierSource` enum (Caster/Target) enables source-aware scaling.
  - DamageEffect + DOTEffect both resolve modifiers; code patterns parallel.
  - Demo attributes created (health, mana, ability_power).

3. [x] **DOT reapply idempotency** (2026-03-29)
- Evidence:
  - `DOTEffect.ApplyModifiers()` calls `_damagePerTick.ClearModifiers()` before re-adding.
  - `OverTimeEffectLifetimeManager.ReApplyOverTimeEffectsModifier(target)` new public API.
  - Enemy.TakeDamage example demonstrates reapply trigger.

4. [x] **Snapshot + dynamic damage validation** (2026-03-29)
- Evidence:
  - Damage = (base + caster ability_power mods) * stacks = snapshot.
  - TakeDamage applies target defenses = dynamic per call.
  - Tested with Fireball on Enemy; confirmed 100-zombie scenario impossible.

5. [x] **Event signaling on attributes** (2026-03-29)
- Evidence:
  - `Attribute.OnRuntimeValueChanged` + `OnBaseValueChanged` events added.
  - `ConsumableAttribute.OnCurrentAmountChanged` event added.
  - ModifierApplication invokes events when values change (for UI binding readiness).

6. [x] **IAttributeHolder contract finalized** (2026-03-29)
- Evidence:
  - Renamed `IAttributeBearer` → `IAttributeHolder` for clarity.
  - Player + Enemy both implement; separate consumable + regular attribute dicts.
  - `TryGetAttribute()`, `CanConsumeCost()`, `ConsumeCost()`, `RegisterAttributes()` all present.

7. [x] **String attribute name fragility accepted (MVP)** (2026-03-29)
- Evidence:
  - Runtime lookup remains exact-name by design.
  - Documented in README as known limitation + mitigation path.
  - Authoring works smoothly; runtime typos fail at effect apply (acceptable for MVP).

8. [x] **AbilityCost → Attribute migration** (earlier session)
- Evidence:
  - `AbilityCostDefinition` removed (legacy files deleted).
  - `AbilityDefinition.Costs` now `List<AttributeDefinition>`.
  - Player/Enemy consume costs via Attribute API (cleaner contract).

9. [x] **Effect apply outcome reporting** (earlier session)
- Evidence:
  - `Assets/AbilitySystem/Effects/Runtime/IAbilityEffect.cs` returns `AbilityEffectApplyResult`.
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/ApplyEffectAction.cs` aggregates applied/skipped/failed counts.

10. [x] **Repeat action context isolation** (earlier session)
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs` creates sub-runners with `context.Fork()`.

11. [x] **Demo contract cleanup for enemy target** (earlier session)
- Evidence:
  - `Assets/Demo/Player/Enemy.cs` implements `IAttributeHolder` (and `IAbilityTarget`, `IDamageable`).
  - Removed `NotImplementedException` stubs.

12. [x] **Ability lifecycle disposal baseline** (earlier session)
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs` exposes `Dispose()`.
  - `Assets/Demo/Player/Player.cs` disposes abilities in `OnDestroy()`.

13. [x] **Editor asset-based attribute reference validation** (2026-03-30)
- Evidence:
  - `Assets/AbilitySystem/Editor/Validation/AttributeReferenceAssetValidator.cs` scans authored attribute and modifier assets.
  - Validation reports unknown modifier references with close-name suggestions.
  - Runtime attribute registry/disposal bookkeeping removed from Attribute/Player/Enemy runtime paths.
- Outcome:
  - Validation is deterministic and no longer coupled to runtime object lifetime or load order.

14. [x] **Modifier resolution helper extraction** (2026-03-30)
- Evidence:
  - `Assets/AbilitySystem/Effects/Runtime/ModifierResolutionHelper.cs` added.
  - `Assets/AbilitySystem/Effects/Runtime/DamageEffect.cs` and `Assets/AbilitySystem/Effects/Runtime/DOTEffect.cs` now call shared helper.
- Outcome:
  - Caster/Target binding logic is centralized and easier to extend for new effect types.

15. [x] **Baseline modifier/effect tests added** (2026-03-30)
- Evidence:
  - `Assets/Tests/AttributeModifierAndEffectsTests.cs` added.
  - Tests cover priority ordering, DOT reapply idempotency, effect-instance isolation, and single-instance reuse compounding behavior.
- Outcome:
  - Core modifier behavior is guarded by automated EditMode tests.

16. [x] **Over-time policy split (stacking vs duration)** (2026-03-31)
- Evidence:
  - `DurationPolicyDefinition`, `IDurationPolicy`, and `BasicDurationPolicy` added.
  - `OverTimeEffectDefinition` now has `_durationPolicy` alongside `_stackingPolicy`.
  - `BasicStackingPolicy` no longer owns duration refresh logic.
- Outcome:
  - Clear separation of concerns between stack behavior and duration behavior.

17. [x] **Over-time policy regression tests expanded** (2026-03-31)
- Evidence:
  - `Assets/Tests/AttributeModifierAndEffectsTests.cs` now includes:
    - manager snapshot behavior test for duration-on-preexisting effects
    - first-apply stacking behavior test across stacking modes
    - same-source duration extension behavior test
- Outcome:
  - Core stacking/duration interaction paths are now protected against regressions.

18. [x] **Tick iteration safety hardening** (2026-03-31)
- Evidence:
  - `OverTimeEffectLifetimeManager.Tick(...)` migrated from nested foreach loops to indexed loops over snapshots.
- Outcome:
  - Prevents collection-modification issues when effects expire/unregister during ticking.

## Suggested Next Task (Priority Order)

### Immediate (Session N+1)
1. **Explicit duration policy asset wiring** (P1, Effort S): remove fallback-default dependence in authored content.
2. **CI/pre-build validator automation** (P1, Effort S): run editor asset validator as part of pipeline.
3. **ApplyEffectAction integration tests** (P2, Effort M): end-to-end stat flow.

### Medium-term (Session N+2/N+3)
4. **Consumable clamping rules** (P2, Effort S): Ensure Current ≤ RuntimeValue always.
5. **Cast null-hardening** (P2, Effort S): Safe Execute/Cancel/Interrupt with null runner.
6. **Scene cleanup for managers** (P2, Effort M): Singleton dict lifecycle safety.

### Nice-to-have (Session N+4+)
7. **Event binding examples** (P3, Effort M): UI stat display patterns.
8. **Full integration test suite** (P3, Effort M): Stat flow → damage → target receiv.
