# Ability System (Assets/AbilitySystem)

## Overview
Small, data-driven ability framework for Unity. Abilities are authored as ScriptableObject "definitions" and executed as ordered runtime actions using an action pipeline and a shared `AbilityContext`. Designed for designer-friendly authoring and flexible runtime composition with **stat scaling via flexible modifier system**.

## Core Concepts
- **Definition → Runtime**: ScriptableObject definitions create runtime objects (e.g., `DamageEffectDefinition` → `DamageEffect`).
- **Action pipeline**: `AbilityDefinition` contains an ordered `List<AbilityActionDefinition>`. `AbilityInstance` builds `IAbilityAction` objects and runs them with `AbilityRunner.Next()`.
- **AbilityContext**: typed common fields (`Caster`, `TargetPoint`, `Targets`) plus a blackboard (`Set` / `TryGet`) for custom data. Now passed to effects at construction.
- **Effects**: implement `IAbilityEffect`. Receive `AbilityContext` at construction for stat/level/metadata access. Instant effects apply immediately; Over-time effects support ticking DoT/buff logic with full modifier support.
- **Projectiles**: world entities that resolve hits and notify the pipeline via events (e.g., `OnHit(HitData)`).
- **Attributes & Modifiers**: Stat system with base/runtime values, priority-ordered modifier composition, and source-aware (Caster/Target) scaling for flexible damage/heal effects.

## Attributes & Modifiers (Stat System)

### Design Overview
The Attributes/Modifiers system provides flexible stat scaling for damage, healing, and effects. Core principle: **Snapshot offensive stats at effect creation; apply defensive reactions at impact**.

### Key Components

**Attribute Runtime:**
- `Attribute`: Base + Runtime values. Modifiers compose via priority-ordered visitor pattern.
- `ConsumableAttribute`: Extends Attribute with separate `CurrentAmount` (for health, mana, ammo).
- `BaseValue`: Base stat (e.g., health max = 100).
- `RuntimeValue`: Base + sum of all modifiers applied in priority order.

**Definitions (ScriptableObjects):**
- `AttributeDefinition`: Creates runtime `Attribute` objects. Authored with name, base value.
- `ConsumableAttributeDefinition`: Creates `ConsumableAttribute` with separate max/current setup.
- `AttributeModifierDefinition`: Scales another attribute by percent. Configured with:
  - **Priority**: Order of application (lower first).
  - **Source**: `Caster` or `Target` (resolved per effect apply).
  - **Attribute Name**: Target attribute to scale from (e.g., "ability_power").
  - **Percent**: Scale factor (entered as 0–100, converts to 0.0–1.0 internally).
  - **Strategy**: Which value to read (Base/Runtime/Current).

**ModifierSource Enum:**
- `Caster`: Bonus attribute comes from source (attacker).  
- `Target`: Bonus attribute comes from target (defender).

**Modifier Application Strategies:**
- `Base`: Scales `bonusAttribute.BaseValue * percent`.
- `Runtime`: Scales `bonusAttribute.RuntimeValue * percent` (includes other active modifiers).
- `Current`: Scales `consumableAttribute.CurrentAmount * percent` (e.g., health or ammo).

### Example: Damage Scaling

**Setup:**
1. Create `AbilityPower` AttributeDefinition (base 100).
2. Create `Fireball_DamageModifier_AbilityPower` AttributeModifierDefinition:
   - Source: Caster
   - Attribute Name: "ability_power"
   - Strategy: Runtime
   - Percent: 10 (= 0.1)
   - Priority: 1
3. Add modifier to DamageEffectDefinition.

**Runtime:**
1. Caster has ability_power = 100 + other modifiers = 120 runtime.
2. Fireball deals 50 base damage.
3. At apply: `damage * (caster.ability_power * 0.1)` = 50 + (120 * 0.1) = 62 total.
4. Target applies armor/shields on TakeDamage (defensive reeval).

### Snapshot vs Dynamic
- **Snapshot**: Offensive stats (ability_power, level) evaluated once at effect creation.
- **Dynamic**: Defensive stats (armor, shields) evaluated on each hit/tick.
- Why: Prevents 100-zombie stacking when enemy armor updates mid-DoT.

### Over-Time Effects with Modifiers

DOT effects snapshot damage-per-tick with full modifier support:

```csharp
// DOTEffect constructor
_damagePerTick = new Attribute(baseTickDamage);
// Then ApplyModifiers() binds bonus attributes (e.g., caster ability_power)
// and adds them to _damagePerTick
```

**Reapply Pattern:**
- Call `OverTimeEffectLifetimeManager.ReApplyOverTimeEffectsModifier(target)` when source/target stats change.
- DOT clears old modifiers → re-evaluates bonus attributes → re-adds them.
- Safe by design: no duplicate stacking.

### Design Choices Explained

1. **String keys for attribute names** (pragmatic MVP):
   - Trade: Typo fragility.
   - Gain: Designer-friendly authoring (no SO selector for every role).
   - Mitigation: Planned startup validator (TODO).

2. **Instance-scoped modifier resolution**:
   - Each effect gets its own modifier list.
   - No global state or inter-ability contamination.
   - Scales cleanly with abilities cast in parallel.

3. **Event signaling** (optional for now):
   - `OnRuntimeValueChanged`, `OnBaseValueChanged` events on Attribute.
   - Enables future UI/animation binding without threading it everywhere.

## Quick Start (FireBall demo)
1. Create an `AbilityDefinition` SO and add `actionDefinitions` in order:
   - `SetCenterActionDefinition` — writes `TargetPoint` into the `AbilityContext`.
   - `TargetingActionDefinition` — e.g., `AOECircleTargetingStrategyDefinition`.
   - `ApplyEffectActionDefinition` (Damage).
   - `ApplyEffectActionDefinition` (DOT).
2. Construct an `AbilityInstance` in gameplay code:
   ```csharp
   var instance = new AbilityInstance(abilityDefinition, caster);
   ```
3. Trigger the ability (demo `FireBall` shows usage). For runtime data, either use a setup action that writes into the context or configure the context via a callback before running the pipeline.
4. Expected flow: targeting resolves → damage applies (with modifiers) → DOT attaches (with modifiers scaled to current stats) and ticks.

### Modifiers at authoring time:
1. Create `AttributeModifierDefinition` SOs (e.g., Fireball_DamageModifier_AbilityPower).
2. Assign them to effect definitions (DamageEffectDefinition._modifiers list).
3. Designers edit percent scale in inspector (e.g., 10 = 10% of caster ability power).
4. Runtime: effects resolve caster/target attributes and apply modifiers automatically.

## Extension Points
- Add new actions: create an `AbilityActionDefinition` SO + runtime `IAbilityAction`.
- Add targeting strategies: implement `ITargetingStrategy` + `TargetingStrategyDefinition`.
- Add effects: create `AbilityEffectDefinition` + runtime `IAbilityEffect` (now receives `AbilityContext` at construction).
- Add custom projectiles: subclass `Projectile`, override `Launch`, emit `OnHit(HitData)`.
- Add custom modifiers: Implement `IModifier`; create `ModifierDefinition` factory. Compose on Attributes via `AddModifier()`.
- Add stat-change listeners: Subscribe to `Attribute.OnRuntimeValueChanged` or `OnBaseValueChanged` events for UI/animations.

## Advanced Patterns

### Effect → Attribute → Modifier Flow

```csharp
// In ApplyEffectAction
var effect = effectDefinition.CreateEffect(context); // context passed; effect can read caster level, metadata, etc.
effect.ApplyTo(target); // effect resolves modifiers and scales damage

// Inside DamageEffect.ApplyTo
foreach (var modifier in _modifiers) {
    if (modifier is AttributeModifier attrMod) {
        // Resolve source attribute (Caster or Target)
        if (attrMod.Source == ModifierSource.Caster && sourceAttributeHolder != null) {
            sourceAttributeHolder.TryGetAttribute(attrMod.AttributeName, out var sourceAttr);
            attrMod.SetBonusAttribute(sourceAttr); // Bind the source attribute
            _damageAmount.AddModifier(attrMod); // Apply via visitor
        }
    }
}
damageable.TakeDamage(_damageAmount.RuntimeValue, _context.Caster); // Snapshot damage sent to target
```

### Registering Attributes on Actors

Implement `IAttributeHolder`:
```csharp
public class Player : MonoBehaviour, IAttributeHolder {
    private Dictionary<string, ConsumableAttribute> _consumableAttributes = new();
    private Dictionary<string, Attribute> _attributes = new();
    
    void Awake() {
        RegisterAttributes();
    }
    
    public void RegisterAttributes() {
        var health = _healthDef.CreateRuntimeConsumableAttribute();
        _consumableAttributes.Add(health.Name, health);
        
        var abilityPower = _abilityPowerDef.CreateRuntimeAttribute();
        _attributes.Add(abilityPower.Name, abilityPower);
    }
    
    public bool TryGetAttribute(string name, out Attribute attr) {
        if (_consumableAttributes.TryGetValue(name, out var cons)) {
            attr = cons;
            return true;
        }
        return _attributes.TryGetValue(name, out attr);
    }
}
```

### Runtime Stat Changes

Update a stat and modifiers recompose automatically:
```csharp
attribute.SetBaseValue(newValue); // triggers RecalculateRuntimeValues()
// Or for consumables:
consumableAttr.Consume(amount); // updates CurrentAmount, fires OnCurrentAmountChanged
```

For DOT reapply after stat change:
```csharp
// In TakeDamage or elsewhere when stats change
OverTimeEffectLifetimeManager.Instance.ReApplyOverTimeEffectsModifier(target);
// DOTs re-evaluate caster/target bonus attributes and recalc damage-per-tick
```

## Gotchas & Notes

- `AbilityInstance` should null-check `actionDefinitions` for older assets.
- **String attribute names are case-sensitive** and unvalidated at authoring time. Plan to add startup validator for typos.
- **Modifier priority is global per Attribute**, not per effect. If multiple effects apply modifiers to the same attribute, they compose in priority order.
- `ConsumableAttribute` max is stored in `RuntimeValue` (base + modifiers). Current amount is separate `_current`. If RuntimeValue changes, Current clamps to new max.
- **Snapshot semantics**: Damage/heal amounts lock in at effect creation. Target defensive logic (armor, shields) re-evaluates each impact. This prevents 100-zombie stacking.
- DOT reapply: Call `ReApplyOverTimeEffectsModifier()` if source/target stats change mid-effect. Safe by design (clears old modifiers before re-adding).
- Prefer `IAttributeHolder` for stat containers instead of concrete type checks.
- Use the blackboard for optional, ability-specific data; keep common fields strongly typed on `AbilityContext`.
- For pipeline-pausing projectiles, ensure `OnHit` updates context before `runner.Next()` is called.

## Known Limitations

- **String-key fragility (MVP pragmatism)**: Attribute names are strings. Typos fail at runtime. Recommended stop-gap: startup validation that warns on unknown attribute names.
- **No modifier removal tracking**: Modifiers are added and never explicitly removed from Attributes during an effect. When effects expire, they leave modifiers behind (by design for snapshot). Consider if long-lived effects need cleanup.
- **No dynamic regen system yet**: Attributes can change, but no built-in regeneration loop. Regen would need a ticker subscribing to stat change events or a periodic effect (regeneration ability).

## Recommended Next Priority Tasks
- Decide on next modifier type(s): flat bonuses, conditional modifiers, or ability-level scaling.
- Add startup validator for attribute name typos (check all modifier definitions resolve against known attributes).
- Extract shared modifier resolution logic from DamageEffect + DOTEffect into helper (reduce duplication).
- Add clamping rules for consumable attributes (e.g., mana ≥ 0, health ≥ 0).
- Test suite for stat changes, modifier composition, and DOT reapply scenarios.
- Projectile model decision: self-contained vs pipeline-pausing. Recommendation: pipeline-pausing via `OnHit(HitData)` so the runner resumes after impact.
