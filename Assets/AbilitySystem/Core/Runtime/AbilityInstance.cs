
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance
{
    private AbilityDefinition _definition;
    private ICaster _caster;
    private float _cooldownRemaining;
    private List<IAbilityEffect> _effects;
    public ITargetingStrategy TargetingStrategy { get; private set; }
    public AbilityInstance(AbilityDefinition definition, ICaster caster, List<IAbilityEffect> activeEffects = null, ITargetingStrategy targetingStrategy = null)
    {
        _definition = definition;
        _caster = caster;
        TargetingStrategy = targetingStrategy;
        _effects = activeEffects ?? new List<IAbilityEffect>();
        _cooldownRemaining = 0f;
    }

    public bool IsOnCooldown()
    {
        return _cooldownRemaining > 0f;
    }

    public void Cast()
    {
        if (!IsOnCooldown() && _caster.CanConsumeCost(_definition.costs))
        {
            Debug.Log($"Casting {_definition.abilityName}");
            _caster.ConsumeCost(_definition.costs);
            _cooldownRemaining = _definition.cooldown;
            IAbilityTarget[] targets = TargetingStrategy.GetTargets().ToArray();

            Debug.Log($"Found {targets.Length} targets for {_definition.abilityName}");
            foreach (var target in targets)
            {
                foreach (var effect in _effects)
                {
                    if (target.CanApplyEffect(effect))
                    {
                        target.ApplyEffect(effect);
                    }
                }
            }
        }
    }
}