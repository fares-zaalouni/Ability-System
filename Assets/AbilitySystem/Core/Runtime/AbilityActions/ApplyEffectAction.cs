using UnityEngine;
using AbilitySystem.Effects;
using AbilitySystem.Targeting;

namespace AbilitySystem.Core
{
    public class ApplyEffectAction : IAbilityAction
    {
        private AbilityEffectDefinition _abilityEffectDefinition;

        public ApplyEffectAction(AbilityEffectDefinition abilityEffectDefinition)
        {
            _abilityEffectDefinition = abilityEffectDefinition;
        }

        public void Execute(AbilityContext context, AbilityRunner runner)
        {
            
            foreach (IAbilityTarget target in context.Targets)
            {
                Debug.Log("Applying effect: " + _abilityEffectDefinition.name + " to target: " + target);
                _abilityEffectDefinition.CreateEffect(context.Caster).ApplyTo(target);
            }
            runner.Next();
        }
    }
}