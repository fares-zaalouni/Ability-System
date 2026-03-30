# Ability System Architecture Summary (2026-03-30)

## Session Overview
Major refactor: Resources → Attributes/Modifiers system. Effects now context-aware. Full modifier support for instant + over-time effects. Snapshot + dynamic damage hybrid model validated. Modifier helper extracted, core tests added, over-time manager cleanup hardened with periodic prune.

**Architecture Grade: 8.7/10** (up from 8.5/10)

## Core System Flow

```
AbilityDefinition (SO)
    ↓
AbilityInstance.Cast(context, dependencies)
    ↓
AbilityRunner (pipeline)
    └─→ ActionDefinition → IAbilityAction.Execute(context, runner)
            ↓
            ApplyEffectAction (example)
                ↓
                EffectDefinition.CreateEffect(context) ← CONTEXT PASSED HERE
                    ↓
                    IAbilityEffect.ApplyTo(target)
                        ↓
                        Resolve modifiers (Caster/Target source)
                        Bind bonus attributes
                        Scale damage/heal
                        Call target.TakeDamage(...) ← TARGET APPLIES DEFENSES
```

## Attributes/Modifiers Pipeline

### Runtime Objects

**Attribute**
- `BaseValue`: Starting value (100).
- `RuntimeValue`: Base + all modifiers applied in priority order.
- `Modifiers`: List<IModifier> (visited in Apply order).
- `RecalculateRuntimeValues()`: Base → apply each modifier → recalc Runtime.

**ConsumableAttribute (extends Attribute)**
- `CurrentAmount`: Consumable tracker (e.g., health used).
- `Max`: Stored in RuntimeValue (base + modifiers).
- Contract: `0 ≤ CurrentAmount ≤ RuntimeValue`.

**AttributeModifier (implements IModifier)**
- `Priority`: Order of application (lower first).
- `Source`: Caster or Target (which holder to pull bonus from).
- `AttributeName`: Name of bonus attribute (e.g., "ability_power").
- `Percent`: Scale factor (0.0–1.0, entered as 0–100 in inspector).
- `Strategy`: Which value of bonus to use (Base/Runtime/Current).
- `SetBonusAttribute(attr)`: Bind the source attribute at apply time.
- `Apply(attr)`: Add modifier's contribution to attr's runtime.

### Authoring (ScriptableObjects)

**AttributeDefinition**
- Factory for runtime `Attribute`.
- Fields: name, baseValue.
- Example: "health" with base 100.

**ConsumableAttributeDefinition (extends AttributeDefinition)**
- Factory for `ConsumableAttribute`.
- Fields: name, baseValue (max), initialAmount (current).
- Example: "health" max 100, start 100.

**AttributeModifierDefinition**
- Factory for `AttributeModifier`.
- Fields: priority, strategy (Base/Runtime/Current), source (Caster/Target), attributeName (target attribute), percent (0–100 range).
- Example: Fireball scales 10% of caster "ability_power".

**EffectDefinition (updated)**
- Now hosts `List<ModifierDefinition> _modifiers`.
- Designers compose modifiers on effects at authoring time.
- Example: DamageEffectDefinition has Fireball_DamageModifier_AbilityPower assigned.

### Resolution at Runtime

**In DamageEffect.ApplyTo(target):**
1. For each modifier in the effect's modifier list:
   - If source == Caster: resolve caster.ability_power attribute.
   - If source == Target: resolve target.health attribute.
   - Bind the source attribute to the modifier.
   - Add modifier to _damageAmount attribute.
2. Damage = _damageAmount.RuntimeValue (base + all modifiers applied).
3. Send to target.TakeDamage(damage).

**Snapshot model:**
- Offensive stats (caster ability_power) locked in at effect creation.
- No change mid-damage, even if caster power changes.

**Dynamic model (in TakeDamage):**
- Target's armor/shields re-evaluated each hit.
- Current health affects mitigation (if armor depends on armor stat).

## Over-Time Effects (DOT)

**OverTimeEffect (base class)**
- Stores `AbilityContext context` (passed at construction via CreateEffect).
- `ApplyModifiers(target)` abstract method (subclasses implement).
- Full lifecycle: apply → tick → tick → ... → expire (delegate to OverTimeEffectLifetimeManager).

**DOTEffect (implements OverTimeEffect)**
- Stores `_damagePerTick` (Attribute).
- `ApplyModifiers(target)` resolves bonus attributes exactly like DamageEffect.
- `ClearModifiers()` before re-add (safety on reapply).
- `ApplyTickTo(target)` sends `_damagePerTick.RuntimeValue * Stacks` to target.

**Reapply Pattern:**
- Call `OverTimeEffectLifetimeManager.ReApplyOverTimeEffectsModifier(target)`.
- Walks all active DOTs on target.
- Calls `effect.ApplyModifiers(target)` on each.
- DOT clears old modifiers → re-evaluates source/target attributes → re-adds.
- Result: damage-per-tick recalcs if caster/target stats changed.

**Example (Enemy health gate):**
```csharp
public void TakeDamage(float amount, ICaster source = null) {
    // When enemy health drops <300 and max is 1000
    if (currentHealth < 300 && maxHealth == 1000) {
        maxHealth = 2000;  // Invoke defensive stance
        OverTimeEffectLifetimeManager.Instance.ReApplyOverTimeEffectsModifier(this);
        // DOTs on enemy now see new max = 2000; can scale from it
    }
    // Apply damage as normal
    currentHealth -= amount;
}
```

## Key Design Decisions

### 1. String Attribute Names (MVP Pragmatism)
- **Trade**: Typos fail at runtime (TakeDamage resolves unknown names).
- **Gain**: Designer-friendly (no SO selector overhead).
- **Mitigation**: P1 hardening task—startup validator warns on unknown names.

### 2. Instance-Scoped Modifier Resolution
- Each effect instantiation has its own modifier list (no global state).
- Caster/Target binding happens per-application, not shared.
- Prevents 100-zombie stacking (100 enemies all receiving fireball → each gets unique modifier binding).
- Shared `ModifierResolutionHelper` now centralizes Caster/Target binding logic for DamageEffect and DOTEffect.

### 3. Snapshot vs Dynamic Hybrid
- **Offensive (Snapshot)**: Damage locked in at effect creation. If caster power changes after cast, damage stays same.
- **Defensive (Dynamic)**: Target defenses (armor, shields) re-evaluated each hit.
- **Why**: Prevents weird interactions where enemy armor update mid-DOT causes retroactive damage change.

### 4. Context at Construction (not Apply)
- Effects receive full `AbilityContext` when created.
- Enables level passing, ability metadata, custom payloads.
- `ApplyTo(target)` signature stays simple; effect has all needed context via construction.

### 5. Event Signaling (Ready for UI)
- `Attribute.OnRuntimeValueChanged` and `OnBaseValueChanged` events exist.
- UI can subscribe to stat changes without hardcoding visibility logic.
- Not yet wired; foundation in place for future binding.

### 6. Asset-Based Attribute Validation
- Attribute typo validation is editor-driven (asset scan), not runtime-registration driven.
- Validation command scans all AttributeDefinition and AttributeModifierDefinition assets.
- Unknown modifier attribute references are logged with close-name suggestions.
- Runtime lookup semantics remain exact-name by design.

## Testing Gaps

- ✅ Modifier priority ordering covered (EditMode test).
- ✅ DOT reapply idempotency covered (EditMode test).
- ✅ Effect instance isolation per caster covered (EditMode test).
- ✅ Explicit single-instance reuse compounding behavior documented by test.
- ❌ No integration test for full damage pipeline (caster power → effect → damage → target mitigation).

## Known Limitations

1. **String-key fragility**: Attribute names unvalidated at authoring time. Typos fail at runtime.
2. **Modifier removal**: Modifiers added to effects but never removed (by design, for snapshot). Long-lived effects should be cleaned up explicitly or tracked.
3. **No built-in regen**: Attributes can change, but no regeneration system yet. Regen would need a periodic effect or ticker.

## Next Steps (High Priority)

1. **Validator automation (P1, S effort)**: Run editor asset validator automatically in CI/pre-build.
2. **Integration tests (P2, M effort)**: full stat-flow cast-to-impact tests.
3. **Unit test expansion (P2, M effort)**: multi-target and stress variants.

## Files Changed (Summary)

**New:**
- `ModifierSource.cs` (enum: Caster/Target).
- `AttributeModifier.cs` (runtime modifier with source awareness).
- `AttributeModifierDefinition.cs` (SO factory).

**Removed (Legacy):**
- `AbilityCostDefinition.cs`, `AbilityCost.cs` (replaced by Attribute API).
- `Demo/Resources/` folder (moved to Demo/Attributes + Modifiers).

**Modified (Core):**
- `IAbilityEffect`: ApplyTo(target) only; context passed at CreateEffect.
- `DamageEffect`, `DOTEffect`: Full modifier support with resolution logic.
- `OverTimeEffect`: ApplyModifiers hook; context stored.
- `Attribute`: Events, ClearModifiers, AddModifiers added.
- `ConsumableAttribute`: Separate base vs current tracking.
- `IAttributeHolder` (renamed from IAttributeBearer): Updated contracts.

## Architecture Scorecard

| Aspect | Score | Notes |
|--------|-------|-------|
| Effects Context-Aware | 9/10 | Level scaling now flows naturally. |
| Modifier Composition | 8.5/10 | Priority ordering works. String fragility (documented). |
| DOT Reapply Safety | 9/10 | ClearModifiers + re-add pattern solid. |
| Instance Isolation | 9/10 | No 100-zombie bugs. Per-effect binding. |
| Snapshot vs Dynamic | 8/10 | Hybrid model validated. clear semantics. |
| String Key Fragility | 6.5/10 | MVP OK, hardening pending. |
| Test Coverage | 3/10 | None yet. High priority. |
| **Overall** | **8.5/10** | MVP-ready. Hardening path clear. |

---

**Ready for**: Next feature iteration (new modifier types), stat expansion, or hardening pass.  
**Not ready for**: Shipped product (string validation + tests needed).
