# Pre-Stats/Modifiers Critical Checklist

Last Updated: 2026-03-17
Purpose: single source of truth for blockers that should be fixed before building Stats & Modifiers.

## How to use this file
- Keep statuses updated as work is completed.
- Add short verification notes after each change.
- Prefer small PRs/commits per checklist item.

## Current Status

### Open Critical Blockers

1. [ ] Resource regeneration does not run
- Severity: Critical
- Why it matters: Stats/modifiers for regen have no runtime effect.
- Evidence:
  - `Assets/AbilitySystem/Resources/Runtime/BaseResource.cs` has `RegenAmount` but no `Tick`/regen apply path.
  - `Assets/AbilitySystem/Resources/Runtime/IResource.cs` has no regen contract.
  - No resource regen call site in `Update`/`FixedUpdate` loops.
- Suggested fix:
  - Add `Tick(float deltaTime)` or equivalent regen API on runtime resource.
  - Call it from an owner loop (caster component or resource manager).

2. [ ] Scene-safe cleanup for singleton managers is missing
- Severity: Critical
- Why it matters: stale caster/target references can persist across scene reloads and corrupt cooldown/effect state.
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/Cooldown/CooldownManager.cs` uses `DontDestroyOnLoad` and keeps dictionaries keyed by caster.
  - `Assets/AbilitySystem/Effects/Runtime/OverTimeEffects/OverTimeEffectLifetimeManager.cs` uses `DontDestroyOnLoad` and keeps dictionaries keyed by target.
  - No `SceneManager.sceneLoaded`/scene-unload cleanup path.
- Suggested fix:
  - Add scene lifecycle cleanup hooks or explicit per-owner unregister on destroy.

### Open High-Priority Blockers

3. [ ] Ability removal does not unregister cooldown entries
- Severity: High
- Why it matters: stale cooldown entries remain if abilities are removed/swapped.
- Evidence:
  - `Assets/Demo/Player/Player.cs` `GrantAbility` registers cooldown.
  - `Assets/Demo/Player/Player.cs` `RemoveAbility` does not call `UnregisterCooldown`.
- Suggested fix:
  - Unregister cooldown before removing ability instance.

4. [ ] Enemy ICaster implementation is stubbed
- Severity: High
- Why it matters: enemy-as-caster scenarios crash if used by future stats/testing.
- Evidence:
  - `Assets/Demo/Player/Enemy.cs` methods throw `NotImplementedException` for ICaster contract.
- Suggested fix:
  - Implement ICaster methods or remove ICaster from Enemy until needed.

5. [ ] Player effect application is stubbed
- Severity: Medium
- Why it matters: player-targeted effects/modifiers cannot be applied.
- Evidence:
  - `Assets/Demo/Player/Player.cs` `CanApplyEffect` and `ApplyEffect` throw `NotImplementedException`.
- Suggested fix:
  - Implement effect acceptance/application path for player target.

## Recently Fixed (Verified)

1. [x] Cast history cleanup in AbilityInstance
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityInstance.cs` removes cast on `OnCancelled`, `OnCompleted`, `OnInterrupted`.

2. [x] Sub-runner callback lifecycle consolidation
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/SubRunnerSubscriptions.cs` used by sub-runner actions.

3. [x] Re-entrancy-safe silent stop API names
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/AbilityRunner.cs` exposes:
    - `StopSilentlyAsCancelled()`
    - `StopSilentlyAsInterrupted()`
  - Call sites updated in:
    - `Assets/AbilitySystem/Core/Runtime/AbilityActions/DoOnSignalAction.cs`
    - `Assets/AbilitySystem/Core/Runtime/AbilityActions/RepeatAction.cs`

4. [x] Explicit sub-runner cleanup mode to prevent hidden aftermath bypass
- Evidence:
  - `Assets/AbilitySystem/Core/Runtime/SubRunnerCleanupMode.cs` defines explicit policy.
  - `Assets/AbilitySystem/Core/Runtime/SubRunnerSubscriptions.cs` applies requested aftermath with selected mode.
  - `Assets/AbilitySystem/Core/Definition/AbilityActions/DoOnSignalActionDefinition.cs` exposes mode in inspector.
  - `Assets/AbilitySystem/Core/Definition/AbilityActions/RepeatActionDefinition.cs` exposes mode in inspector.
  - Default mode set to `RespectChildAftermath` for both actions.

## Suggested Fix Order
1. Resource regen baseline
2. Manager scene-lifecycle cleanup
3. Ability unregister path
4. Enemy ICaster completion
5. Player effect application completion

## Notes
- This list tracks blockers before Stats/Modifiers feature development.
- If scope changes, add a short dated note under this section and update statuses above.
