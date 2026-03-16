using UnityEngine;
using AbilitySystem.Core;

namespace AbilitySystem.Effects
{
    [CreateAssetMenu(fileName = "DOTEffectDefinition", menuName = "Ability System/Effects/DOTEffectDefinition")]
    public class DOTEffectDefinition : OverTimeEffectDefinition
    {
        [SerializeField] private float _damageAmount;
        
        public override IAbilityEffect CreateEffect(ICaster source)
        {
            return new DOTEffect(this, _damageAmount, _duration, _tickInterval, _initialStacks,_maxStacks, source);
        }
    }
}
