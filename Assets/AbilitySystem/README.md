# Ability System (Assets/AbilitySystem)

## Overview
Small, data-driven ability framework for Unity. Abilities are authored as ScriptableObject "definitions" and executed as ordered runtime actions using an action pipeline and a shared `AbilityContext`. Designed for designer-friendly authoring and flexible runtime composition.

## Core Concepts
- **Definition → Runtime**: ScriptableObject definitions create runtime objects (e.g., `DamageEffectDefinition` → `DamageEffect`).
- **Action pipeline**: `AbilityDefinition` contains an ordered `List<AbilityActionDefinition>`. `AbilityInstance` builds `IAbilityAction` objects and runs them with `AbilityRunner.Next()`.
- **AbilityContext**: typed common fields (`Caster`, `TargetPoint`, `Targets`) plus a blackboard (`Set` / `TryGet`) for custom data.
- **Effects**: implement `IAbilityEffect`. Instant effects apply immediately; `StatusEffect` supports ticking DoT/buff logic.
- **Projectiles**: world entities that resolve hits and notify the pipeline via events (e.g., `OnHit(HitData)`).

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
4. Expected flow: targeting resolves → damage applies → DOT attaches and ticks.

## Projectiles (recommended pattern)
- Create `ProjectileDefinition` (SO) to configure prefab, speed, and effect definitions.
- Implement a `Projectile` base class with virtual `Launch`/movement and an `OnHit(HitData)` event (payload: hit point, hit targets).
- `SpawnProjectileAction` subscribes to `OnHit(hit)` and updates the `AbilityContext` then calls `runner.Next()`:
  ```csharp
  projectile.OnHit += hit => {
      context.TargetPoint = hit.Point;
      context.SetTargets(hit.Targets);
      runner.Next();
  };
  ```
- Keep projectiles decoupled from `AbilityContext` (projectile supplies `HitData`; the action mediates context updates).

## Extension Points
- Add new actions: create an `AbilityActionDefinition` SO + runtime `IAbilityAction`.
- Add targeting strategies: implement `ITargetingStrategy` + `TargetingStrategyDefinition`.
- Add effects: create `AbilityEffectDefinition` + runtime `IAbilityEffect`.
- Add custom projectiles: subclass `Projectile`, override `Launch`, emit `OnHit(HitData)`.

## Gotchas & Notes
- `AbilityInstance` should null-check `actionDefinitions` for older assets.
- `StatusEffect` ticking: final tick may slightly overshoot to negative `RemainingDuration` due to fixed-delta timing—this is expected unless you require strict semantics.
- Prefer `IStatusEffectReceiver` for status-receiving logic instead of concrete MonoBehaviour checks.
- Use the blackboard for optional, ability-specific data; keep common fields strongly typed on `AbilityContext`.
- For pipeline-pausing projectiles, ensure `OnHit` updates context before `runner.Next()` is called.

## Recommended Next Tasks
- Decide projectile model (self-contained vs pipeline-pausing). Recommendation: pipeline-pausing via `OnHit(HitData)` so the runner resumes after impact.
- Replace string-based resource keys with stronger keys (SO/enum) to avoid silent typos.
- Move ability management (add/remove abilities) out of `ICaster` into a separate owner/component.
- Add editor validation for `AbilityDefinition` SOs and `actionDefinitions` ordering.

## Examples & Patterns
- Example `SpawnProjectileAction` behavior:
  ```csharp
  // spawn projectile; subscribe to its hit event; update context and resume pipeline
  projectile.OnHit += hit => {
      context.TargetPoint = hit.Point;
      context.SetTargets(hit.Targets);
      runner.Next();
  };
  ```

## Contact / Next Help
If you want, I can generate `ProjectileDefinition` and `Projectile` skeleton files ready to drop into the repo, or produce an editor validation script that warns about empty `actionDefinitions`. Which would you like next?
