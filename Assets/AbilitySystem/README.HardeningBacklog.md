# Ability System Hardening Backlog

Last Updated: 2026-03-29
Purpose: single source of truth for architecture hardening and MVP → production blockers.

## Scoring Model
- Priority: P0 (critical).0 (critical), P1 (high), P2 (medium), P3 (low)
- Effort: S (0.5-1 day), M (1-3 days), L (3-5 days)
- Impact: H (high risk reduction), M (moderate), L (low)

## Open Tasks

1. **P1** Attribute name validation at startup (string-key fragility)
- Effort: S
- Impact: H
- Why:
  - Attribute names are strings (e.g., "ability_power"). Typos fail at runtime.
  - Modifiers reference attributes by name with no validation at authoring time.
- Touch points:
  - `Assets/AbilitySystem/Attributes and Modifiers/Runtime/`
  - New util: AttributeNameValidator (check all modifier definitions resolve)
- Acceptance criteria:
  - On startup, log warning for each modifier that references unknown attribute names.
  - Suggest nearest-match for typos (future: quick-fix in editor).

2. **P1** Extract shared modifier resolution helper (code deduplication)
- Effort: S  
- Impact: M
- Why:
  - DamageEffect + DOTEffect double the modifier resolution code (Caster/Target binding).
  - Will triple/quadruple when adding Heal/Shield/Buff effects.
- Touch points:
  - `Assets/AbilitySystem/Effects/Runtime/DamageEffect.cs`
  - `Assets/AbilitySystem/Effects/Runtime/DOTEffect.cs`
  - New util: ModifierResolutionHelper or similar
- Acceptance criteria:
  - Single method handles modifier binding for any effect type.
  - DamageEffect + DOTEffect both call it; 15+ lines saved each.

3. **P2** Modifier composition unit tests
- Effort: M
- Impact: H
- Why:
  - Core stat scaling has no automated validation.
  - Priority ordering, stacking, and reapply are critical paths.
- Touch points:
  - `Assets/Tests/` or new `Assets/AbilitySystem/Tests/Modifiers/`
- Acceptance criteria:
  - Test: modifier priority ordering is respected.
  - Test: modifier reapply is idempotent (same output on N calls).
  - Test: no cross-contamination between DamageEffect instances.
  - Test: 100-zombie scenario (parallel abilities, shared target—ensure no stacking).

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
  - No validator implemented yet (listed as P1 hardening task).
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

## Suggested Next Task (Priority Order)

### Immediate (Session N+1)
1. **Attribute name validator** (P1, Effort S): Warn on missing attributes at startup.
2. **Modifier resolution helper** (P1, Effort S): Extract DamageEffect + DOTEffect duplication.
3. **Modifier composition tests** (P2, Effort M): Priority ordering, reapply, cross-contamination.

### Medium-term (Session N+2/N+3)
4. **Consumable clamping rules** (P2, Effort S): Ensure Current ≤ RuntimeValue always.
5. **Cast null-hardening** (P2, Effort S): Safe Execute/Cancel/Interrupt with null runner.
6. **Scene cleanup for managers** (P2, Effort M): Singleton dict lifecycle safety.

### Nice-to-have (Session N+4+)
7. **Event binding examples** (P3, Effort M): UI stat display patterns.
8. **Full integration test suite** (P3, Effort M): Stat flow → damage → target receiv.
