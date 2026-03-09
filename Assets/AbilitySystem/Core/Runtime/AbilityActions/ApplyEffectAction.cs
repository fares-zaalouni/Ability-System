
using UnityEngine;

public class ApplyEffectAction : IAbilityAction
{
    private AbilityEffectDefinition _abilityEffectDefinition;
    private IAbilityEffect _abilityEffect;

    public ApplyEffectAction(AbilityEffectDefinition abilityEffectDefinition)
    {
        _abilityEffectDefinition = abilityEffectDefinition;
    }

    public void Execute(AbilityContext context, AbilityRunner runner)
    {
        if(_abilityEffect == null)
        {
            _abilityEffect = _abilityEffectDefinition.CreateEffect(context.Caster);
        }
        
        foreach (IAbilityTarget target in context.Targets)
        {
            _abilityEffect.ApplyTo(target);
        }
        runner.Next();
    }
}