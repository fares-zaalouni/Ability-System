
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance
{
    private AbilityDefinition _definition;
    private ICaster _caster;
    private float _cooldownRemaining;
    public ITargetingStrategy TargetingStrategy { get; private set; }
    public AbilityInstance(AbilityDefinition definition, ICaster caster, ITargetingStrategy targetingStrategy = null)
    {
        _definition = definition;
        _caster = caster;
        TargetingStrategy = targetingStrategy;
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
            List<IAbilityTarget> targets = TargetingStrategy.GetTargets();

            Debug.Log($"Found {targets.Count} targets for {_definition.abilityName}");
            foreach (var target in targets)
            {
                foreach (var effectDefinition in _definition.effectDefinitions)
                {
                    var effect = effectDefinition.CreateEffect(_caster);
                    if (target.CanApplyEffect(effect))
                    {
                        effect.ApplyTo(target);
                    }
                }
            }
        }
    }
}