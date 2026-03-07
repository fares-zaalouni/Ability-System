
using System.Collections.Generic;
using UnityEngine;

public class AbilityInstance
{
    private AbilityDefinition _definition;
    private List<IAbilityAction> _actions; 
    private AbilityRunner _runner;
    private ICaster _caster;
    private float _cooldownRemaining;
    public AbilityInstance(AbilityDefinition definition, ICaster caster)
    {
        _actions = new List<IAbilityAction>();
        AbilityContext context = new AbilityContext(caster);
        foreach (var actionDef in definition.actionDefinitions)
        {
            _actions.Add(actionDef.CreateRuntimeAction());
        }
        _runner = new AbilityRunner(_actions, context);
        _definition = definition;
        _caster = caster;
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
            //List<IAbilityTarget> targets = TargetingStrategy.GetTargets();

            /*Debug.Log($"Found {targets.Count} targets for {_definition.abilityName}");*/
            /*foreach (var target in targets)
            {
                foreach (var effectDefinition in _definition.effectDefinitions)
                {
                    var effect = effectDefinition.CreateEffect(_caster);
                    if (target.CanApplyEffect(effect))
                    {
                        effect.ApplyTo(target);
                    }
                }
            }*/
            _runner.Next();
        }
    }
}