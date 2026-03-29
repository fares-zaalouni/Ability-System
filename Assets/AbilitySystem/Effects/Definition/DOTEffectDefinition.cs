using UnityEngine;
using AbilitySystem.Core;
using AbilitySystem.Attributes;
using System.Collections.Generic;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "DOTEffectDefinition", menuName = "Ability System/Effects/DOTEffectDefinition")]
    public class DOTEffectDefinition : OverTimeEffectDefinition
    {
        [SerializeField] private float _damageAmount;
        
        public override IAbilityEffect CreateEffect(AbilityContext context)
        {
            return new DOTEffect(this, _damageAmount, _duration, _tickInterval, _initialStacks,_maxStacks, _modifiers.ConvertAll(m => m.CreateRuntimeModifier()), context);
        }
    }
}
